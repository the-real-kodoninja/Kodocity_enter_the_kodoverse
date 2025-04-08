using UnityEngine;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float GetWalletBalance() { return walletBalance; }
    public float GetVaultBalance() { return vaultBalance; }
    private Rigidbody rb;
    private ICClient icClient;
    private CitadelSurveillance citadel;
    private float health = 100f;
    private bool hasInsurance = false;
    private float cryptoBalance = 10f;
    private bool isDead = false;
    private Vector3 spawnPoint = new Vector3(0, 1, 0);

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        icClient = FindObjectOfType<ICClient>();
        citadel = FindObjectOfType<CitadelSurveillance>();
        if (icClient == null)
        {
            GameObject icClientObj = new GameObject("ICClient");
            icClient = icClientObj.AddComponent<ICClient>();
        }

        RegisterPlayer();
        LoadPlayerData();
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        transform.Translate(move);

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        UpdatePosition();
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    async void RegisterPlayer()
    {
        string result = await icClient.CallCanister("registerPlayer", null);
        Debug.Log($"Player registration result: {result}");
    }

    async void LoadPlayerData()
    {
        string result = await icClient.QueryCanister("getPlayer", BackendConfig.PlayerId);
        if (!string.IsNullOrEmpty(result))
        {
            var playerData = JsonUtility.FromJson<PlayerData>(result);
            transform.position = new Vector3(playerData.position.x, playerData.position.y, playerData.position.z);
            health = playerData.health;
            hasInsurance = playerData.hasInsurance;
            cryptoBalance = playerData.cryptoBalance;
            Debug.Log($"Loaded player data - Health: {health}, Insurance: {hasInsurance}, Balance: {cryptoBalance}");
        }
    }

    async void UpdatePosition()
    {
        await icClient.CallCanister("updatePosition", transform.position.x, transform.position.y, transform.position.z);
    }

    public async void TakeDamage(float damage, string source)
    {
        if (isDead) return;

        health = Mathf.Max(0, health - damage);
        Debug.Log($"Player took {damage} damage from {source}. Health: {health}");

        if (health <= 0)
        {
            Die();
        }
        await icClient.CallCanister("updatePlayerHealthAndBalance", health, cryptoBalance);
    }

    async void Die()
    {
        isDead = true;
        Debug.Log("Player has died!");
        if (hasInsurance && cryptoBalance >= 5.0) // Revival costs 5 ICP
        {
            cryptoBalance -= 5.0;
            await icClient.CallCanister("updatePlayerHealthAndBalance", 100.0, cryptoBalance);
            Revive();
        }
        else
        {
            Debug.Log("No insurance or insufficient funds. Progress wiped!");
            // Wipe progress (reset position, balance, etc.)
            cryptoBalance = 0;
            await icClient.CallCanister("updatePlayerHealthAndBalance", 100.0, cryptoBalance);
            transform.position = spawnPoint;
            isDead = false;
        }
    }

    void Revive()
    {
        transform.position = spawnPoint; // In the future, spawn at a Medic Center
        health = 100f;
        isDead = false;
        Debug.Log("Player revived by Medic!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RestrictedArea"))
        {
            citadel.PlayerEnteredRestrictedArea();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RestrictedArea"))
        {
            citadel.PlayerExitedRestrictedArea();
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string id;
    public Position position;
    public int[] ownedLands;
    public string agentId;
    public float health;
    public bool hasInsurance;
    public float cryptoBalance; // Deprecated
    public float walletBalance;
    public float vaultBalance;
}