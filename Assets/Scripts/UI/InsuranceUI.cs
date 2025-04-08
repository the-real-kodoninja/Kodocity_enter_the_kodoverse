using UnityEngine;
using UnityEngine.UI;

public class InsuranceUI : MonoBehaviour
{
    public Button purchaseInsuranceButton;
    private ICClient icClient;

    void Start()
    {
        icClient = FindObjectOfType<ICClient>();
        purchaseInsuranceButton.onClick.AddListener(PurchaseInsurance);
    }

    async void PurchaseInsurance()
    {
        string result = await icClient.CallCanister("purchaseInsurance");
        if (!string.IsNullOrEmpty(result) && result.Contains("true"))
        {
            Debug.Log("Insurance purchased successfully!");
        }
        else
        {
            Debug.Log("Failed to purchase insurance. Insufficient funds?");
        }
    }
}
