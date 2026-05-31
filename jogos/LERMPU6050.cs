using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MPU6050Controller : MonoBehaviour
{
    // Importações de funções nativas do Linux/Android via libc
    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int LinuxOpen(string filename, int flags);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int LinuxClose(int fd);

    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    private static extern int LinuxIoctl(int fd, int request, int arg);

    [DllImport("libc", EntryPoint = "read", SetLastError = true)]
    private static extern int LinuxRead(int fd, byte[] buf, int count);

    [DllImport("libc", EntryPoint = "write", SetLastError = true)]
    private static extern int LinuxWrite(int fd, byte[] buf, int count);

    // Constantes do Linux para comunicação I2C
    private const int O_RDWR = 0x0002;
    private const int I2C_SLAVE = 0x0703;

    // Endereço padrão da MPU6050
    private const string I2C_DEVICE = "/dev/i2c-1";
    private const int MPU6050_ADDRESS = 0x68;

    // Registradores da MPU6050
    private const byte PWR_MGMT_1 = 0x6B;
    private const byte ACCEL_XOUT_H = 0x3B;

    private int fileDescriptor = -1;

    void Start()
    {
        InitializeSensor();
    }

    void InitializeSensor()
    {
        // 1. Abre o arquivo do barramento I2C
        fileDescriptor = LinuxOpen(I2C_DEVICE, O_RDWR);
        if (fileDescriptor < 0)
        {
            Debug.LogError($"[MPU6050] Falha ao abrir o barramento {I2C_DEVICE}. O SELinux foi desativado e o chmod 666 foi aplicado?");
            return;
        }

        // 2. Define o endereço do escravo (MPU6050)
        if (LinuxIoctl(fileDescriptor, I2C_SLAVE, MPU6050_ADDRESS) < 0)
        {
            Debug.LogError("[MPU6050] Falha ao comunicar com o endereço 0x68.");
            CloseConnection();
            return;
        }

        // 3. Tira a MPU6050 do modo "Sleep" (Escreve 0 no registrador 0x6B)
        WriteRegister(PWR_MGMT_1, 0x00);
        Debug.Log("[MPU6050] Conectado e inicializado com sucesso!");
    }

    void Update()
    {
        if (fileDescriptor < 0) return;

        // Lê 14 bytes sequenciais a partir do registrador 0x3B (Acelerômetro, Temperatura e Giroscópio)
        byte[] buffer = ReadSequence(ACCEL_XOUT_H, 14);

        if (buffer != null && buffer.Length == 14)
        {
            // Processa os dados brutos de 16 bits (combina byte High e byte Low)
            short rawAccelX = (short)((buffer[0] << 8) | buffer[1]);
            short rawAccelY = (short)((buffer[2] << 8) | buffer[3]);
            short rawAccelZ = (short)((buffer[4] << 8) | buffer[5]);
            
            short rawGyroX = (short)((buffer[8] << 8) | buffer[9]);
            short rawGyroY = (short)((buffer[10] << 8) | buffer[11]);
            short rawGyroZ = (short)((buffer[12] << 8) | buffer[13]);

            // Conversão opcional: Divide por 16384.0 para obter valores em Força G (configuração padrão de ±2g)
            float accelX = rawAccelX / 16384.0f;
            float accelY = rawAccelY / 16384.0f;
            float accelZ = rawAccelZ / 16384.0f;

            // Usa os valores para mover objetos no Unity
            MoveGameObjects(accelX, accelY);
        }
    }

    private void MoveGameObjects(float x, float y)
    {
        // Exemplo: Aplica a inclinação do sensor na movimentação de um objeto
        Vector3 movement = new Vector3(x, 0, y) * Time.deltaTime * 5f;
        transform.Translate(movement);
    }

    // Função auxiliar para escrever em um registrador do sensor
    private void WriteRegister(byte register, byte value)
    {
        byte[] buffer = new byte[] { register, value };
        LinuxWrite(fileDescriptor, buffer, buffer.Length);
    }

    // Função auxiliar para ler uma sequência de bytes do sensor
    private byte[] ReadSequence(byte startRegister, int length)
    {
        // Informa ao sensor qual registrador queremos começar a ler
        byte[] regBuf = new byte[] { startRegister };
        if (LinuxWrite(fileDescriptor, regBuf, 1) < 0) return null;

        // Lê a quantidade de bytes solicitada
        byte[] dataBuf = new byte[length];
        if (LinuxRead(fileDescriptor, dataBuf, length) < 0) return null;

        return dataBuf;
    }

    private void CloseConnection()
    {
        if (fileDescriptor >= 0)
        {
            LinuxClose(fileDescriptor);
            fileDescriptor = -1;
            Debug.Log("[MPU6050] Conexão I2C fechada.");
        }
    }

    void OnDestroy()
    {
        CloseConnection();
    }
}
