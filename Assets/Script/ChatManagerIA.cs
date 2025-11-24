using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatRequest
{
    public ChatMessage[] messages;
}

[System.Serializable]
public class AssistantResponse
{
    public ChatMessage assistant;
}

public class ChatManagerIA : MonoBehaviour
{
    [Header("Referências de UI")]
    public TMP_InputField inputField;
    public Transform contentArea;
    public GameObject userMessagePrefab;
    public GameObject botMessagePrefab;
    public TMP_Text avisoLimiteTexto;

    [Header("Avisos e carregamento")]
    public GameObject avisoTexto;

    [Header("Configuração do Servidor")]
    public string apiUrl = "https://chatconnect-server-hye5cec2a5gef2f8.eastus2-01.azurewebsites.net/api/chat";

    [Header("Limites e controle de delay")]
    public int maxInputLength = 40;
    public float delayEntreMensagens = 3f;

    [Header("Painel e botões de chat")]
    public GameObject painelChat;
    public GameObject botaoAbrirChat;
    public GameObject botaoFecharChat;

    private bool aguardandoResposta = false;

    void Start()
    {
        if (inputField != null)
        {
            inputField.characterLimit = maxInputLength;
            inputField.onValueChanged.AddListener(OnInputChanged);
        }

        if (avisoLimiteTexto != null)
            avisoLimiteTexto.gameObject.SetActive(false);

        if (avisoTexto != null)
            avisoTexto.SetActive(false);

        if (painelChat != null)
            painelChat.SetActive(false);

        if (botaoFecharChat != null)
            botaoFecharChat.SetActive(false);

        if (botaoAbrirChat != null)
            botaoAbrirChat.SetActive(true);
    }

    public void AbrirChat()
    {
        if (painelChat != null) painelChat.SetActive(true);
        if (botaoAbrirChat != null) botaoAbrirChat.SetActive(false);
        if (botaoFecharChat != null) botaoFecharChat.SetActive(true);
    }

    public void FecharChat()
    {
        if (painelChat != null) painelChat.SetActive(false);
        if (botaoAbrirChat != null) botaoAbrirChat.SetActive(true);
        if (botaoFecharChat != null) botaoFecharChat.SetActive(false);

        ApagarMensagens();
    }

    void OnInputChanged(string currentText)
    {
        if (avisoLimiteTexto == null) return;

        if (currentText.Length >= maxInputLength)
        {
            avisoLimiteTexto.text = "Limite de caracteres atingido!";
            avisoLimiteTexto.gameObject.SetActive(true);
        }
        else
        {
            avisoLimiteTexto.gameObject.SetActive(false);
        }
    }

    public void OnSendMessage()
    {
        if (aguardandoResposta)
        {
            
            if (avisoTexto != null && !avisoTexto.activeSelf)
                StartCoroutine(MostrarAvisoTemporario("Aguarde um pouco pra enviar novamente.", 2f));
            return;
        }

        string userInput = inputField.text.Trim();
        if (string.IsNullOrEmpty(userInput)) return;

        inputField.text = "";

        ChatRequest req = new ChatRequest
        {
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "user", content = userInput }
            }
        };

        StartCoroutine(SendChatPair(req, userInput));
    }

    IEnumerator SendChatPair(ChatRequest request, string userInput)
    {
        aguardandoResposta = true;

        string json = JsonUtility.ToJson(request);
        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                AddMessagePair(userInput, "Erro: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Resposta: " + jsonResponse);

                AssistantResponse data = JsonUtility.FromJson<AssistantResponse>(jsonResponse);
                string botResponse = (data != null && data.assistant != null)
                    ? data.assistant.content
                    : "Erro ao interpretar resposta.";

                AddMessagePair(userInput, botResponse);
            }
        }

        yield return new WaitForSeconds(delayEntreMensagens);
        aguardandoResposta = false;
    }

    IEnumerator MostrarAvisoTemporario(string texto, float duracao)
    {
        avisoTexto.SetActive(true);
        avisoTexto.GetComponent<TMP_Text>().text = texto;
        yield return new WaitForSeconds(duracao);
        avisoTexto.SetActive(false);
    }

    void AddMessagePair(string userText, string botText)
    {
        GameObject userMsg = Instantiate(userMessagePrefab, contentArea);
        userMsg.SetActive(true);
        userMsg.GetComponentInChildren<TMP_Text>().text = userText;

        GameObject botMsg = Instantiate(botMessagePrefab, contentArea);
        botMsg.SetActive(true);
        botMsg.GetComponentInChildren<TMP_Text>().text = botText;

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        yield return null;
        Canvas.ForceUpdateCanvases();

        ScrollRect scroll = contentArea.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea.GetComponent<RectTransform>());
            scroll.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }

    void LateUpdate()
    {
        if (contentArea != null)
        {
            var pos = contentArea.GetComponent<RectTransform>().anchoredPosition;
            pos.x = 0;
            contentArea.GetComponent<RectTransform>().anchoredPosition = pos;
        }
    }

    void ApagarMensagens()
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        Canvas.ForceUpdateCanvases();
    }
}
