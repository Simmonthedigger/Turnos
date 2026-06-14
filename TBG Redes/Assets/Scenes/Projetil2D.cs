using UnityEngine;

public class Projetil2D : MonoBehaviour
{
    private TimeEquipe timeAliado;
    private float danoProjetil;
    private bool configurado = false;
    private Collider2D meuCollider; // Guarda o collider do próprio projétil

    void Awake()
    {
        // Pega o collider do próprio tiro assim que ele nasce
        meuCollider = GetComponent<Collider2D>();
    }

    // Este método será chamado pelo personagem logo após instanciar o tiro
    public void ConfigurarProjetil(TimeEquipe timeAtirador, float danoDoPersonagem)
    {
        timeAliado = timeAtirador;
        danoProjetil = danoDoPersonagem;
        configurado = true;
    }

    // Funciona se os colliders forem físicos (sólidos)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessarImpacto(collision.gameObject, collision.collider);
    }

    // Funciona se os colliders forem Trigger (fantasmas)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessarImpacto(collision.gameObject, collision);
    }

    private void ProcessarImpacto(GameObject objetoAtingido, Collider2D colliderAtingido)
    {
        if (!configurado) return;

        // 1. VERIFICA SE JÁ ATINGIU O CENÁRIO
        if (objetoAtingido.CompareTag("Cenario"))
        {
            Debug.Log("O tiro colidiu com o cenário e foi destruído.");
            Destroy(gameObject);
            return;
        }

        // Tenta pegar o componente de personagem do objeto atingido
        PersonagemTurno2D inimigo = objetoAtingido.GetComponent<PersonagemTurno2D>();

        if (inimigo != null)
        {
            // 2. EVITA FOGO AMIGO FÍSICO
            if (inimigo.timeAtual == timeAliado)
            {
                // Diz para a Unity desconsiderar totalmente o contato físico entre esses dois colliders
                if (meuCollider != null && colliderAtingido != null)
                {
                    Physics2D.IgnoreCollision(meuCollider, colliderAtingido);
                }
                return; 
            }
            
            // 3. SE FOR INIMIGO: Aplica dano e se destrói
            else
            {
                Debug.Log($"O tiro acertou um inimigo do time {inimigo.timeAtual} causando {danoProjetil} de dano!");
                
                // Chama a função de dano do script do personagem
                inimigo.ReceberDano(danoProjetil);

                // Destrói o projétil após o impacto bem-sucedido
                Destroy(gameObject);
            }
        }
    }
}