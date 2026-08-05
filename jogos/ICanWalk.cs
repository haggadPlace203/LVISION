using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GyroKalman : MonoBehaviour
{
    public Vector3 accel = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
    public Vector3 gyro = new Vector3(0.0f, 0.0f, -1.0f);

    public void Init()
    {
        // Habilita os sensores através do New Input System
        if (UnityEngine.InputSystem.Gyroscope.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        }
        else
        {
            Debug.LogWarning("Nenhum giroscópio detectado pelo Input System.");
        }

        if (UnityEngine.InputSystem.Accelerometer.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
        }
    }

    public void Update()
    {
        // Verifica se os sensores estão ativos e disponíveis nesta frame
        if (UnityEngine.InputSystem.Gyroscope.current != null && UnityEngine.InputSystem.Accelerometer.current != null)
        {
            // Lê aceleração bruta
            this.accel = UnityEngine.InputSystem.Accelerometer.current.acceleration.ReadValue();

            // Lê a velocidade angular comum (mais segura que a versão Unbiased em ROMs customizadas)
            this.gyro = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();

            float dt = Time.deltaTime;

            // Regra da mão esquerda do Unity
            Quaternion deltaRotation = Quaternion.Euler(-this.gyro.x * Mathf.Rad2Deg * dt, -this.gyro.y * Mathf.Rad2Deg * dt, this.gyro.z * Mathf.Rad2Deg * dt);

            this.rotation *= deltaRotation;
        }
    }
}

public class ICanWalk : MonoBehaviour
{
    [Tooltip("O componente de corpo rígido (RigidBody) do jogador.")]
    public Rigidbody rigid;
    [Tooltip("Os olhos do jogador.")]
    public GameObject Camera;
    [Tooltip("Permite que a I.A. controle o jogador.")]
    public Vector3 externalInput;
    [Tooltip("Permite o jogador ser livre.")]
    public bool allowUserInput;
    [Tooltip("Posição inicial após reiniciar a posição do jogador. Não coloque acima de uma região sem solo.")]
    public Vector3 startPosition;
    [Tooltip("A altura inferior necessária para o jogador voltar à posição inicial.")]
    public float minimumResetHeight;
    [Tooltip("Força aplicada ao RigidBody para a locomoção do jogador.")]
    public float speed;
    Vector3 myDirection;
    GyroKalman gyro;

    // Criamos as ações direto como variáveis privadas
    private InputAction moveAction;
    private InputAction jumpAction;

    private void Awake()
    {
        // 1. Configurando a ação de Movimento (Vector2) com binding para WASD
        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value,
            binding: "<Gamepad>/leftStick" // Controle como primeira opção
        );

        // Adicionando o teclado (WASD) como um composto direto pelo código
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // 2. Configurando a ação de Pulo (Botão)
        jumpAction = new InputAction(
            name: "Jump",
            type: InputActionType.Button
        );

        // Adicionando os botões de pulo (Espaço e botão Sul do controle)
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        // 3. Registrando a função que roda no momento do clique (Pulo)
        // jumpAction.performed += ctx => Jump();
    }

    private void OnEnable()
    {
        // É obrigatório ativar as ações manualmente para começarem a escutar
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        // É boa prática desativar para evitar vazamento de memória
        moveAction.Disable();
        jumpAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        myDirection = new Vector3(0.0f, 0.0f, 0.0f);

        // Adiciona o script de giroscópio dinamicamente e o inicializa
        gyro = gameObject.AddComponent<GyroKalman>();
        gyro.Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (allowUserInput)
        {
            // Captura o Pitch (X) e Yaw (Y) originais do Mouse
            float mouseYaw = Input.mousePosition.x;
            float mousePitch = Mathf.Min(Mathf.Max(-(float)(Input.mousePosition.y - (Screen.height >> 1)), -85.0f), 85.0f);

            // Obtém as rotações do giroscópio (caso ele esteja ativo)
            Quaternion gyroRotation = gyro != null ? gyro.rotation : Quaternion.identity;
            float gyroYaw = gyroRotation.eulerAngles.y;

            // Rotação do corpo: Mouse (Yaw) combinado com o Giroscópio (Yaw) para a caminhada e cálculo de matriz
            this.transform.rotation = Quaternion.Euler(0.0f, mouseYaw, 0.0f) * Quaternion.Euler(0.0f, gyroYaw, 0.0f);

            // O cálculo da matriz original foi preservado
            Matrix4x4 dirMat = new Matrix4x4();
            dirMat[0, 0] = Mathf.Cos(this.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            dirMat[0, 1] = Mathf.Sin(this.transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
            dirMat[1, 0] = -dirMat[0, 1];
            dirMat[1, 1] = dirMat[0, 0];

            // Aplicação do movimento baseada na matriz preservada
            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 directionToWalk = dirMat * new Vector2(input.x, input.y).normalized * speed;
            myDirection = new Vector3(directionToWalk.x, 0.0f, directionToWalk.y);

            // Rotação da câmera: Combina os eixos restritos do Mouse com a rotação livre 3D do Giroscópio
            Quaternion mouseCamRotation = Quaternion.Euler(mousePitch, mouseYaw, 0.0f);
            Camera.transform.rotation = mouseCamRotation * gyroRotation;
        }

        myDirection += externalInput;
        rigid.AddForce(myDirection);

        if (this.transform.position.y <= minimumResetHeight)
        {
            rigid.velocity = Vector3.zero;
            this.transform.position = startPosition;
        }
    }

    private void FixedUpdate()
    {
        // A atualização do GyroKalman já está ocorrendo em seu próprio método Update herdado de MonoBehaviour
    }
}