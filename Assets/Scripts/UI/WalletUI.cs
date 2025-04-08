using UnityEngine;
using UnityEngine.UI;

public class WalletUI : MonoBehaviour
{
    public Text walletBalanceText;
    public Text vaultBalanceText;
    public InputField transferAmountInput;
    public Button transferToVaultButton;
    public Button withdrawFromVaultButton;
    private ICClient icClient;
    private PlayerController player;

    void Start()
    {
        icClient = FindObjectOfType<ICClient>();
        player = FindObjectOfType<PlayerController>();
        transferToVaultButton.onClick.AddListener(TransferToVault);
        withdrawFromVaultButton.onClick.AddListener(WithdrawFromVault);
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    async void TransferToVault()
    {
        if (float.TryParse(transferAmountInput.text, out float amount) && amount > 0)
        {
            string result = await icClient.CallCanister("transferToVault", amount);
            if (!string.IsNullOrEmpty(result) && result.Contains("true"))
            {
                Debug.Log($"Transferred {amount} to vault!");
                // Reload player data
                await player.LoadPlayerData();
            }
            else
            {
                Debug.Log("Failed to transfer to vault. Insufficient funds?");
            }
        }
    }

    async void WithdrawFromVault()
    {
        if (float.TryParse(transferAmountInput.text, out float amount) && amount > 0)
        {
            string result = await icClient.CallCanister("withdrawFromVault", amount);
            if (!string.IsNullOrEmpty(result) && result.Contains("true"))
            {
                Debug.Log($"Withdrew {amount} from vault!");
                await player.LoadPlayerData();
            }
            else
            {
                Debug.Log("Failed to withdraw from vault. Insufficient funds?");
            }
        }
    }

    void UpdateUI()
    {
        walletBalanceText.text = $"Wallet: {player.GetWalletBalance()} ICP";
        vaultBalanceText.text = $"Vault: {player.GetVaultBalance()} ICP";
    }
}
