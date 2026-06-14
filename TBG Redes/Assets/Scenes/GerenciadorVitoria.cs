using UnityEngine;

public class GerenciadorVitoria : MonoBehaviour
{
    private bool jogoFinalizado = false;

    void Update()
    {
        // Se o jogo já acabou, para de checar para não inundar o console
        if (jogoFinalizado) return;

        // Procura na cena todos os personagens que ainda existem/estão vivos
        PersonagemTurno2D[] todosPersonagens = FindObjectsByType<PersonagemTurno2D>(FindObjectsSortMode.None);

        // Se a partida ainda não começou ou não tem ninguém na cena, ignora
        if (todosPersonagens.Length == 0) return;

        bool temVermelhoVivo = false;
        bool temAzulVivo = false;

        // Vasculha a lista para ver quais times ainda têm representantes
        foreach (PersonagemTurno2D personagem in todosPersonagens)
        {
            if (personagem.timeAtual == TimeEquipe.Vermelho)
            {
                temVermelhoVivo = true;
            }
            else if (personagem.timeAtual == TimeEquipe.Azul)
            {
                temAzulVivo = true;
            }
        }

        // --- VERIFICAÇÃO DAS CONDIÇÕES DE VITÓRIA ---

        // Se o time Vermelho sumiu da cena e o Azul continua lá
        if (!temVermelhoVivo && temAzulVivo)
        {
            AnunciarVencedor("Jogador azul ganhou", Color.blue);
        }
        // Se o time Azul sumiu da cena e o Vermelho continua lá
        else if (!temAzulVivo && temVermelhoVivo)
        {
            AnunciarVencedor("Jogador vermelho ganhou", Color.red);
        }
    }

    private void AnunciarVencedor(string mensagem, Color corTexto)
    {
        jogoFinalizado = true;

        // Converte a cor para Hexadecimal para aplicar no sistema de Linhas Rich Text da Unity
        string hexCor = ColorUtility.ToHtmlStringRGB(corTexto);

        // Mostra a mensagem estilizada e em negrito no Console
        Debug.Log($"<color=#{hexCor}><b>{mensagem.ToUpper()}!</b></color>");
    }
}