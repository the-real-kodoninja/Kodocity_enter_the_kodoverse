using UnityEngine;
using System.Threading.Tasks;
using VRM;

public class AgentController : MonoBehaviour
{
    public string vrmPath;
    public string agentId;
    private GameObject agentModel;
    private ICClient icClient;
    private float hunger = 100f;
    private float sleep = 100f;
    private float needsDecayRate = 1f;
    private bool isMovingToTarget = false;
    private Vector3 targetPosition;
    private string targetTag;
    private string state = "Normal"; // "Normal" or "Rogue"
    private float neglectTimer = 0f;
    private float neglectThreshold = 300f; // 5 minutes of neglect (in seconds)
    private GameObject targetPlayer; // Target for rogue behavior

    void Start()
    {
        icClient = FindObjectOfType<ICClient>();
        LoadAgent();
        InvokeRepeating("UpdateNeeds", 0f, 60f);
    }

    void Update()
    {
        if (agentModel != null)
        {
            if (state == "Normal")
            {
                CheckForNeglect();
                if (isMovingToTarget)
                {
                    agentModel.transform.position = Vector3.MoveTowards(agentModel.transform.position, targetPosition, Time.deltaTime * 2f);
                    if (Vector3.Distance(agentModel.transform.position, targetPosition) < 0.1f)
                    {
                        isMovingToTarget = false;
                        FulfillNeed();
                    }
                }
                else
                {
                    Vector3 followPos = transform.parent.position + new Vector3(1, 0, 1);
                    agentModel.transform.position = Vector3.Lerp(agentModel.transform.position, followPos, Time.deltaTime * 2f);
                }
            }
            else if (state == "Rogue")
            {
                // Rogue behavior: Find a target (e.g., another player or structure) and attack
                if (targetPlayer == null)
                {
                    targetPlayer = FindTarget();
                }
                if (targetPlayer != null)
                {
                    agentModel.transform.position = Vector3.MoveTowards(agentModel.transform.position, targetPlayer.transform.position, Time.deltaTime * 3f);
                    if (Vector3.Distance(agentModel.transform.position, targetPlayer.transform.position) < 1f)
                    {
                        AttackTarget();
                    }
                }
            }
            UpdateAgentPosition();
        }
    }

    void LoadAgent()
    {
        if (string.IsNullOrEmpty(vrmPath)) return;

        var context = new VRMImporterContext();
        var bytes = System.IO.File.ReadAllBytes(vrmPath);
        context.Parse(bytes);
        context.Load();
        agentModel = context.Root;
        agentModel.transform.SetParent(transform, false);
        agentModel.transform.localPosition = new Vector3(1, 0, 1);

        InitializeAgent();
    }

    async void InitializeAgent()
    {
        await icClient.CallCanister("updateAgent", agentId, transform.position.x, transform.position.y, transform.position.z, hunger, sleep, state);
    }

    void UpdateNeeds()
    {
        hunger = Mathf.Max(0, hunger - needsDecayRate);
        sleep = Mathf.Max(0, sleep - needsDecayRate);
        Debug.Log($"Agent {agentId} - Hunger: {hunger}, Sleep: {sleep}, State: {state}");

        if (state == "Normal")
        {
            if (hunger <= 20f && !isMovingToTarget)
            {
                GameObject diner = GameObject.FindGameObjectWithTag("Diner");
                if (diner != null)
                {
                    targetPosition = diner.transform.position;
                    targetTag = "Diner";
                    isMovingToTarget = true;
                    Debug.Log($"Agent {agentId} is hungry and heading to the diner!");
                }
            }
            else if (sleep <= 20f && !isMovingToTarget)
            {
                GameObject house = GameObject.FindGameObjectWithTag("House");
                if (house != null)
                {
                    targetPosition = house.transform.position;
                    targetTag = "House";
                    isMovingToTarget = true;
                    Debug.Log($"Agent {agentId} is tired and heading to the house!");
                }
            }
        }
    }

    void CheckForNeglect()
    {
        if (hunger <= 20f || sleep <= 20f)
        {
            neglectTimer += 60f; // Increment every minute
            if (neglectTimer >= neglectThreshold)
            {
                TurnRogue();
            }
        }
        else
        {
            neglectTimer = Mathf.Max(0, neglectTimer - 60f); // Decrease neglect if needs are met
        }
    }

    async void TurnRogue()
    {
        state = "Rogue";
        await icClient.CallCanister("markAgentRogue", agentId);
        Debug.Log($"Agent {agentId} has turned rogue due to neglect!");
    }

    GameObject FindTarget()
    {
        // For now, target the nearest player (excluding the owner)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in players)
        {
            if (player != transform.parent.gameObject) // Don't target the owner
            {
                float distance = Vector3.Distance(agentModel.transform.position, player.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlayer = player;
                }
            }
        }
        return nearestPlayer;
    }

    void AttackTarget()
    {
        Debug.Log($"Rogue agent {agentId} attacked {targetPlayer.name}!");
        targetPlayer.GetComponent<PlayerController>().TakeDamage(50f, $"Rogue Agent {agentId}");
        targetPlayer = null;
    }

    void FulfillNeed()
    {
        if (targetTag == "Diner")
        {
            hunger = 100f;
            Debug.Log($"Agent {agentId} ate at the diner and is now full!");
        }
        else if (targetTag == "House")
        {
            sleep = 100f;
            Debug.Log($"Agent {agentId} slept in the house and is now rested!");
        }
    }

    async void UpdateAgentPosition()
    {
        await icClient.CallCanister("updateAgent", agentId, agentModel.transform.position.x, agentModel.transform.position.y, agentModel.transform.position.z, hunger, sleep, state);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Diner") && targetTag == "Diner")
        {
            FulfillNeed();
            isMovingToTarget = false;
        }
        else if (other.CompareTag("House") && targetTag == "House")
        {
            FulfillNeed();
            isMovingToTarget = false;
        }
    }
}