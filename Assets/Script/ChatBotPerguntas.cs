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
    public GameObject mensagemInicial; 
    public GameObject botaoRoxo;      

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

        botaoAbrirChat.onClick.RemoveAllListeners();
        botaoAbrirChat.onClick.AddListener(() =>
        {
            painelChat.SetActive(true);
            botaoFechar.gameObject.SetActive(true);
            botaoAbrirChat.gameObject.SetActive(false);

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

        botaoFechar.onClick.RemoveAllListeners();
        botaoFechar.onClick.AddListener(() =>
        {
            painelChat.SetActive(false);
            botaoFechar.gameObject.SetActive(false);
            botaoAbrirChat.gameObject.SetActive(true);
        });

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
