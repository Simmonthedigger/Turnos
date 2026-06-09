using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

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

    [Header("Referências de Tiro")]
    public GameObject prefabProjetil;
    public Transform pontoDisparo;

    private Rigidbody2D rb;
    private Vector2 posicaoInicialClique;
    private Vector2 vetorLancamento;
    private bool estaArrastando = false;
    private int modoAcao = 0; // 1 = Movimento (Botão Direito), 2 = Tiro (Botão Esquerdo)

    // Propriedade para o LineRenderer que o Gerenciador vai usar
    public bool EstaArrastando => estaArrastando;
    public Vector2 VetorLancamento => vetorLancamento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Garante que o ponto de disparo não seja nulo
        if (pontoDisparo == null) pontoDisparo = transform;
    }

    void Update()
    {
        // Se este personagem não for o ativo do turno atual, ignora os comandos
        if (GerenciadorTurnos.Instancia.PersonagemAtivo != this) return;

        ProcessarInput();
    }

    private void ProcessarInput()
    {
        // --- BOTÃO DIREITO: MOVIMENTAÇÃO ---
        if (Input.GetMouseButtonDown(1)) // Clique Inicial
        {
            ComeçarArrasto(1);
        }
        // --- BOTÃO ESQUERDO: TIRO (Só se já estiver selecionado e não clicando duas vezes) ---
        else if (Input.GetMouseButtonDown(0) && !GerenciadorTurnos.Instancia.DetectandoCliqueDuplo)
        {
            ComeçarArrasto(2);
        }

        // --- DURANTE O ARRASTO ---
        if (estaArrastando)
        {
            Vector2 posicaoMouseAtual = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // CORRIGIDO: Juntei o "direcaoRaw" para o C# reconhecer como uma única variável
            Vector2 direcaoRaw = posicaoInicialClique - posicaoMouseAtual;

            // Limita o alcance máximo da mira
            vetorLancamento = Vector2.ClampMagnitude(direcaoRaw, alcanceMaximoMira);

            // Finaliza a ação ao soltar o botão correspondente
            if (modoAcao == 1 && Input.GetMouseButtonUp(1))
            {
                ExecutarMovimento();
            }
            else if (modoAcao == 2 && Input.GetMouseButtonUp(0))
            {
                ExecutarTiro();
            }
        }
    }

    private void ComeçarArrasto(int modo)
    {
        estaArrastando = true;
        modoAcao = modo;
        posicaoInicialClique = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
            GameObject proj = Instantiate(prefabProjetil, pontoDisparo.position, Quaternion.identity);
            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();

            if (rbProj != null)
            {
                // Lança o projétil usando a força do arrasto e o alcance/dano como base
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

    // Desenha o alcance máximo no editor da Unity para ajudar no balanceamento
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alcanceMaximoMira);
    }
}