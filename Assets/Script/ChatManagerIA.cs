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
    public Transform contentArea; // Content do ScrollView
    public GameObject userMessagePrefab;
    public GameObject botMessagePrefab;
    public TMP_Text avisoLimiteTexto;

    [Header("Configuração do Servidor")]
    public string apiUrl = "http://localhost:3000/api/chat";

    [Header("Limites de texto")]
    public int maxInputLength = 30;

    [Header("Painel e botões de chat")]
    public GameObject painelChat;      // Painel que contém o chat completo
    public GameObject botaoAbrirChat;  // Ícone do balão
    public GameObject botaoFecharChat; // Ícone X

    void Start()
    {
        if (inputField != null)
        {
            inputField.characterLimit = maxInputLength;
            inputField.onValueChanged.AddListener(OnInputChanged);
        }

        if (avisoLimiteTexto != null)
            avisoLimiteTexto.gameObject.SetActive(false);

        // 🔹 Garante que comece fechado
        if (painelChat != null)
            painelChat.SetActive(false);

        if (botaoFecharChat != null)
            botaoFecharChat.SetActive(false);

        if (botaoAbrirChat != null)
            botaoAbrirChat.SetActive(true);
    }

    // 🟢 Funções para abrir e fechar o chat
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
    }

    void OnInputChanged(string currentText)
    {
        if (avisoLimiteTexto != null)
        {
            if (currentText.Length >= maxInputLength)
            {
                avisoLimiteTexto.text = $"Limite de caracteres atingido!";
                avisoLimiteTexto.gameObject.SetActive(true);
            }
            else
            {
                avisoLimiteTexto.gameObject.SetActive(false);
            }
        }
    }

    public void OnSendMessage()
    {
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
    }

    void AddMessagePair(string userText, string botText)
    {
        Debug.Log("Adicionando mensagens: " + userText + " / " + botText);

        GameObject userMsg = Instantiate(userMessagePrefab, contentArea);
        userMsg.SetActive(true);
        userMsg.GetComponentInChildren<TMP_Text>().text = userText;

        GameObject botMsg = Instantiate(botMessagePrefab, contentArea);
        botMsg.SetActive(true);
        botMsg.GetComponentInChildren<TMP_Text>().text = botText;
    }
}
