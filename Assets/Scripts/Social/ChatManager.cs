using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ChatManager : NetworkBehaviour
{
    public Text chatLog;
    public InputField chatInput;
    private NetworkVariable<string> chatMessage = new NetworkVariable<string>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        chatInput.onEndEdit.AddListener(SendMessage);
        chatMessage.OnValueChanged += (oldValue, newValue) =>
        {
            chatLog.text += $"\n{newValue}";
        };
    }

    void SendMessage(string message)
    {
        if (!string.IsNullOrEmpty(message) && IsOwner)
        {
            chatMessage.Value = $"{NetworkManager.Singleton.LocalClientId}: {message}";
            chatInput.text = "";
        }
    }
}
