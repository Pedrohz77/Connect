using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBotPerguntas : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject painelChat;
    public Button botaoAbrirChat;
    public Button botaoFechar;
    public Transform listaPerguntas;
    public GameObject botaoPerguntaPrefab;
    public TMP_Text textoResposta;
    public ScrollRect scrollRect;

    [Header("Mensagem Inicial e Botão Roxo")]
    public GameObject mensagemInicial; // 👈 Mensagem “Este é o Chat...”
    public GameObject botaoRoxo;       // 👈 Botão roxo que mostra as perguntas

    [Header("Perguntas e Respostas")]
    [TextArea(2, 5)] public string[] perguntas;
    [TextArea(2, 5)] public string[] respostas;

    void Start()
    {
        if (painelChat == null || botaoAbrirChat == null || botaoFechar == null ||
            listaPerguntas == null || botaoPerguntaPrefab == null || textoResposta == null)
        {
            Debug.LogError("⚠️ Faltam referências no ChatBotPerguntas. Verifique o Inspector!");
            return;
        }

        painelChat.SetActive(false);
        botaoFechar.gameObject.SetActive(false);

        // 🔹 Abre o chat
        botaoAbrirChat.onClick.RemoveAllListeners();
        botaoAbrirChat.onClick.AddListener(() =>
        {
            painelChat.SetActive(true);
            botaoFechar.gameObject.SetActive(true);
            botaoAbrirChat.gameObject.SetActive(false);

            // Mostra mensagem inicial e botão roxo
            if (mensagemInicial != null)
                mensagemInicial.SetActive(true);
            if (botaoRoxo != null)
                botaoRoxo.SetActive(true);

            listaPerguntas.gameObject.SetActive(false);
            textoResposta.text = "";

            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        });

        // 🔹 Fecha o chat
        botaoFechar.onClick.RemoveAllListeners();
        botaoFechar.onClick.AddListener(() =>
        {
            painelChat.SetActive(false);
            botaoFechar.gameObject.SetActive(false);
            botaoAbrirChat.gameObject.SetActive(true);
        });

        // 🔹 Cria os botões de perguntas (mas esconde)
        for (int i = 0; i < perguntas.Length; i++)
        {
            int index = i;
            GameObject novoBotao = Instantiate(botaoPerguntaPrefab, listaPerguntas);
            novoBotao.SetActive(true);

            TMP_Text textoBotao = novoBotao.GetComponentInChildren<TMP_Text>();
            if (textoBotao != null)
                textoBotao.text = perguntas[i];

            Button botao = novoBotao.GetComponent<Button>();
            if (botao != null)
                botao.onClick.AddListener(() => MostrarResposta(index));
        }

        // 🔹 Listener para o botão roxo
        if (botaoRoxo != null)
        {
            Button btnRoxo = botaoRoxo.GetComponent<Button>();
            if (btnRoxo != null)
            {
                btnRoxo.onClick.RemoveAllListeners();
                btnRoxo.onClick.AddListener(() => MostrarPerguntas());
            }
        }
    }

    // 🔹 Quando clicar no botão roxo
    public void MostrarPerguntas()
    {
        if (mensagemInicial != null)
            mensagemInicial.SetActive(false);
        if (botaoRoxo != null)
            botaoRoxo.SetActive(false);

        listaPerguntas.gameObject.SetActive(true);
        textoResposta.text = "";
    }

    void MostrarResposta(int index)
    {
        if (index >= 0 && index < respostas.Length)
            textoResposta.text = respostas[index];
        else
            textoResposta.text = "Desculpe, não entendi sua pergunta.";
    }
}
