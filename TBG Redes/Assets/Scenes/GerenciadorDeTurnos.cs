using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class GerenciadorTurnos : MonoBehaviour
{
    public static GerenciadorTurnos Instancia { get; private set; }

    [Header("Configuração de Turno Atual")]
    public TimeEquipe timeDoTurnoAtual = TimeEquipe.Vermelho;

    private PersonagemTurno2D personagemAtivo;
    public PersonagemTurno2D PersonagemAtivo => personagemAtivo;

    [Header("Configurações de Clique Duplo")]
    public float tempoLimiteCliqueDuplo = 0.3f;
    private float ultimoTempoClique = 0f;
    public bool DetectandoCliqueDuplo { get; private set; }

    private LineRenderer linhaTrajetoria;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        linhaTrajetoria = GetComponent<LineRenderer>();
        ConfigurarLineRenderer();
    }

    void Update()
    {
        DetectarSelecaoPersonagem();
        AtualizarLinhaTrajetoria();
    }

    private void DetectarSelecaoPersonagem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float diferencaTempo = Time.time - ultimoTempoClique;

            if (diferencaTempo <= tempoLimiteCliqueDuplo)
            {
                DetectandoCliqueDuplo = true;
                TentarSelecionarPersonagem();
            }
            else
            {
                DetectandoCliqueDuplo = false;
            }

            ultimoTempoClique = Time.time;
        }
    }

    private void TentarSelecionarPersonagem()
    {
        // Lança um Raycast do mouse para detectar se clicou em um personagem 2D
        Vector2 raioMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(raioMouse, Vector2.zero);

        if (hit.collider != null)
        {
            PersonagemTurno2D personagemClicado = hit.collider.GetComponent<PersonagemTurno2D>();

            if (personagemClicado != null)
            {
                // Verifica se o personagem pertence ao time do turno atual
                if (personagemClicado.timeAtual == timeDoTurnoAtual)
                {
                    personagemAtivo = personagemClicado;
                    Debug.Log($"Personagem do Time {personagemAtivo.timeAtual} selecionado e pronto!");
                }
                else
                {
                    Debug.Log($"Não é o turno do Time {personagemClicado.timeAtual}!");
                }
            }
        }
    }

    private void AtualizarLinhaTrajetoria()
    {
        if (personagemAtivo != null && personagemAtivo.EstaArrastando)
        {
            linhaTrajetoria.enabled = true;
            Vector3 posicaoInicial = personagemAtivo.transform.position;
            // Mostra para onde o personagem vai/atira (Posição + Vetor invertido do arrasto)
            Vector3 posicaoFinal = posicaoInicial + (Vector3)personagemAtivo.VetorLancamento;

            linhaTrajetoria.SetPosition(0, posicaoInicial);
            linhaTrajetoria.SetPosition(1, posicaoFinal);
        }
        else
        {
            linhaTrajetoria.enabled = false;
        }
    }

    public void FinalizarTurno()
    {
        // Reseta o personagem ativo para que ele não receba comandos fora do seu turno
        personagemAtivo = null;

        // Alterna o time (Preparado para expansão LAN via RPCs no futuro)
        timeDoTurnoAtual = (timeDoTurnoAtual == TimeEquipe.Vermelho) ? TimeEquipe.Azul : TimeEquipe.Vermelho;

        Debug.Log($"Turno finalizado! Agora é a vez do Time: {timeDoTurnoAtual}");
    }

    private void ConfigurarLineRenderer()
    {
        linhaTrajetoria.positionCount = 2;
        linhaTrajetoria.startWidth = 0.1f;
        linhaTrajetoria.endWidth = 0.05f;
        linhaTrajetoria.material = new Material(Shader.Find("Sprites/Default"));
        linhaTrajetoria.startColor = Color.red;
        linhaTrajetoria.endColor = Color.yellow;
        linhaTrajetoria.enabled = false;
    }
}