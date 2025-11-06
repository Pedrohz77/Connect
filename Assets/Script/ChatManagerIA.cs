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
    public TMP_Text avisoLimiteTexto; // 🔹Texto de aviso (adicionar no inspector)

    [Header("Configuração do Servidor")]
    public string apiUrl = "http://localhost:3000/api/chat";

    [Header("Limites de texto")]
    public int maxInputLength = 30;   //  máximo de caracteres permitidos no input
    public int maxBotWords = 10;       //  máximo de palavras na resposta do bot

    void Start()
    {
        if (inputField != null)
        {
            inputField.characterLimit = maxInputLength;

            // 🔹 Escuta mudança de texto pra mostrar aviso quando atingir o limite
            inputField.onValueChanged.AddListener(OnInputChanged);
        }

        // 🔸 Esconde o aviso no início
        if (avisoLimiteTexto != null)
            avisoLimiteTexto.gameObject.SetActive(false);
    }

    void OnInputChanged(string currentText)
    {
        // Mostra aviso quando atinge o limite
        if (avisoLimiteTexto != null)
        {
            if (currentText.Length >= maxInputLength)
            {
                avisoLimiteTexto.text = $"Limite de caracteres atingido!"; //{maxInputLength}
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

                // 🔹 Limita a resposta da IA a X palavras
                string[] palavras = botResponse.Split(' ');
                if (palavras.Length > maxBotWords)
                {
                    botResponse = string.Join(" ", palavras, 0, maxBotWords) + "...";
                }

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

        // 🔸 Faz o scroll descer automaticamente
        ScrollRect scroll = contentArea.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0;
        }
    }
}
