using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Adicionado para suportar o novo Input System

public enum TimeEquipe { Vermelho, Azul }

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PersonagemTurno2D : MonoBehaviour
{
    [Header("Configurações de Time")]
    public TimeEquipe timeAtual;

    [Header("Atributos do Personagem")]
    public float velocidadeLancamento = 5f;
    public float forcaPuloModificador = 1.2f; // Multiplicador para o pulo/movimento
    public float alcanceMaximoMira = 3f;
    public float dano = 20f;
    public float tempoDeRecarga = 1f; // Para uso futuro de habilidades

    [Header("Atributos de Vida")]
    public float vidaMaxima = 100f;
    private float vidaAtual;

    [Header("Referências de Tiro")]
    public GameObject prefabProjetil;
    public Transform pontoDisparo;

    private Rigidbody2D rb;
    private Vector2 posicaoInicialClique;
    private Vector2 vetorLancamento;
    private bool estaArrastando = false;
    private int modoAcao = 0; // 1 = Movimento (Botão Direito), 2 = Tiro (Botão Esquerdo)

    // Propriedades que o Gerenciador de Turnos usa para desenhar a linha de mira
    public bool EstaArrastando => estaArrastando;
    public Vector2 VetorLancamento => vetorLancamento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Garante que o ponto de disparo não seja nulo
        if (pontoDisparo == null) pontoDisparo = transform;

        // Inicializa a vida do personagem
        vidaAtual = vidaMaxima;
    }

    void Update()
    {
        // SEGURANÇA: Evita NullReferenceException caso o Gerenciador de Turnos ainda não tenha sido carregado
        if (GerenciadorTurnos.Instancia == null) return;

        // Se este personagem não for o ativo do turno atual, ignora os comandos
        if (GerenciadorTurnos.Instancia.PersonagemAtivo != this) return;

        ProcessarInput();
    }

    private void ProcessarInput()
    {
        // Se o mouse não estiver conectado ou ativo por algum motivo, aborta
        if (Mouse.current == null) return;

        // --- BOTÃO DIREITO: MOVIMENTAÇÃO ---
        if (Mouse.current.rightButton.wasPressedThisFrame) // Clique Inicial
        {
            ComeçarArrasto(1);
        }
        // --- BOTÃO ESQUERDO: TIRO (Só se já estiver selecionado e não executando o clique duplo de seleção) ---
        else if (Mouse.current.leftButton.wasPressedThisFrame && !GerenciadorTurnos.Instancia.DetectandoCliqueDuplo)
        {
            ComeçarArrasto(2);
        }

        // --- DURANTE O ARRASTO ---
        if (estaArrastando)
        {
            // Pega a posição do mouse na tela usando o Novo Input System
            Vector2 posicaoMouseTela = Mouse.current.position.ReadValue();
            Vector2 posicaoMouseAtual = Camera.main.ScreenToWorldPoint(posicaoMouseTela);
            
            // Calcula a direção oposta (estilo estilingue/Angry Birds)
            Vector2 direcaoRaw = posicaoInicialClique - posicaoMouseAtual;

            // Limita o alcance máximo da mira
            vetorLancamento = Vector2.ClampMagnitude(direcaoRaw, alcanceMaximoMira);

            // Finaliza a ação ao soltar o botão correspondente
            if (modoAcao == 1 && Mouse.current.rightButton.wasReleasedThisFrame)
            {
                ExecutarMovimento();
            }
            else if (modoAcao == 2 && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ExecutarTiro();
            }
        }
    }

    private void ComeçarArrasto(int modo)
    {
        estaArrastando = true;
        modoAcao = modo;

        Vector2 posicaoMouseTela = Mouse.current.position.ReadValue();
        posicaoInicialClique = Camera.main.ScreenToWorldPoint(posicaoMouseTela);
    }

    private void ExecutarMovimento()
    {
        estaArrastando = false;
        
        // Aplica a força baseada no arrasto multiplicada pela velocidade e pelo modificador de pulo
        Vector2 forcaFinal = vetorLancamento * velocidadeLancamento * forcaPuloModificador;
        rb.AddForce(forcaFinal, ForceMode2D.Impulse);

        // Finaliza o turno da equipe após a ação física
        GerenciadorTurnos.Instancia.FinalizarTurno();
    }

    private void ExecutarTiro()
    {
        estaArrastando = false;

        if (prefabProjetil != null)
        {
            // Cria o projétil na posição correta
            GameObject proj = Instantiate(prefabProjetil, pontoDisparo.position, Quaternion.identity);

            // CONFIGURAÇÃO DO TIRO: Passa o time do atirador e o dano dele para o projétil
            Projetil2D scriptProjetil = proj.GetComponent<Projetil2D>();
            if (scriptProjetil != null)
            {
                scriptProjetil.ConfigurarProjetil(timeAtual, dano);
            }

            // Aplica a física do lançamento
            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
            if (rbProj != null)
            {
                Vector2 forcaTiro = vetorLancamento * velocidadeLancamento;
                rbProj.AddForce(forcaTiro, ForceMode2D.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("Nenhum prefab de projétil atribuído ao personagem!");
        }

        // Finaliza o turno da equipe após o tiro
        GerenciadorTurnos.Instancia.FinalizarTurno();
    }

    // Método público para ser chamado pelo script do projétil inimigo
    public void ReceberDano(float quantidadeDano)
    {
        vidaAtual -= quantidadeDano;
        Debug.Log($"{gameObject.name} (Time {timeAtual}) recebeu {quantidadeDano} de dano. Vida restante: {vidaAtual}");

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        Debug.Log($"{gameObject.name} morreu!");

        // Se o personagem que morreu era o ativo do turno, limpa a referência antes de destruir
        if (GerenciadorTurnos.Instancia.PersonagemAtivo == this)
        {
            GerenciadorTurnos.Instancia.FinalizarTurno();
        }

        Destroy(gameObject);
    }

    // Desenha o alcance máximo no editor da Unity para ajudar no balanceamento
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alcanceMaximoMira);
    }
}