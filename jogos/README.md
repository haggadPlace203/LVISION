# [Nome do Jogo/Projeto]

Uma aplicação de jogo desenvolvida em Unity, otimizada para o sistema operacional Android rodando em hardware Raspberry Pi.

## 📋 Sobre o Projeto

Este projeto consiste em um jogo criado na engine Unity. O foco principal do desenvolvimento foi a otimização de performance e mapeamento de controles para a execução estável em uma placa Raspberry Pi adaptada com LineageOS (Android).

### 🛠️ Tecnologias Utilizadas

*   **Engine:** Unity
*   **Plataforma de Destino:** Android
*   **Hardware Alvo:** Raspberry Pi
*   **Sistema Operacional do Pi:** LineageOS

---

## 🚀 Requisitos e Configuração do Ambiente

### 1. Hardware Necessário
*   Raspberry Pi (Recomendado Pi 4 ou superior)
*   Cartão MicroSD (Classe 10 de no mínimo 16GB)
*   Fonte de alimentação 5V com 3A
*   Periférico de entrada Teclado e Mouse

---

## 🛠️ Como Compilar e Rodar (Desenvolvedores)

### Pré-requisitos no Computador:
*   Unity Hub instalado.
*   Unity Editor com o módulo **Android Build Support** instalado (incluindo NDK e SDK OpenJDK do Android).

### Passos para Build:
1. Clone este repositório:
   ```bash
   git clone https://github.com[seu-usuario]/[seu-repositorio].git
   ```
2. Abra o projeto no Unity Hub.
3. Vá em `File > Build Settings`.
4. Altere a plataforma para **Android** e clique em **Switch Platform**.
5. Em `Player Settings`, certifique-se de que a arquitetura está configurada corretamente para o seu Android no Pi (geralmente **ARM64**).
6. Clique em **Build** e salve o arquivo `.apk`.

### Instalando no Raspberry Pi via ADB:

Transfira o APK para uma unidade móvel e instale conectando-a na porta USB do Raspbery Pi

---

## 🎮 Controles e Input

Como a experiência no Raspberry Pi difere de um celular padrão, os inputs foram mapeados da seguinte forma:

*   **[Ex: Teclas WASD / Setas]:** Movimentação do Personagem.
*   **[Ex: Barra de Espaço]:** Pular.
*   **[Ex: Clique do Mouse]:** Navegação nos Menus.

---

## ⚙️ Otimizações Aplicadas (Performance no Pi)

Devido às limitações de hardware do Raspberry Pi em comparação com smartphones modernos, as seguintes técnicas foram utilizadas:
*   **Graphics API:** Configurado estritamente para **OpenGLES2** ou **OpenGLES3** (Vulkan desativado para maior estabilidade no driver do Pi).
*   **Compressão de Texturas:** Uso de ASTC ou ETC2 para reduzir o consumo de memória VRAM.
*   **Physics/Update:** Redução da taxa de *Fixed Timestep* para aliviar o processamento da CPU.
*   **UI:** Evitado o uso de componentes pesados de Blur e transparências complexas.

---

## ✒️ Autores

*   **Seu Nome** - *Desenvolvimento Geral* - [@seu-usuario](https://github.comseu-usuario)

---
