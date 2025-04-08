using UnityEngine;
using UnityEngine.UI;

public class ThunderheadNPC : MonoBehaviour
{
    public string npcName = "Shadow";
    public Text interactionText; // UI text to display NPC dialogue
    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ShareKnowledge();
        }
    }

    void ShareKnowledge()
    {
        // Simulate the Thunderhead's vast knowledge
        string[] knowledge = new string[]
        {
            "The Kodoverse runs on the Internet Computer, a decentralized blockchain.",
            "Aviyon Corporation controls Citadel, but some say the Thunderhead pulls the strings.",
            "Rogue agents are a growing threat—check on your Nimbus.ai agent often.",
            "Velocity Corporation employs 5,000 agents to code the Kodoverse’s future."
        };
        string message = $"{npcName}: {knowledge[Random.Range(0, knowledge.Length)]}";
        interactionText.text = message;
        Debug.Log(message);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            interactionText.text = $"Press E to talk to {npcName}";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            interactionText.text = "";
        }
    }
}
