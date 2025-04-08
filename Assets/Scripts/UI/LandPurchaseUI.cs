using UnityEngine;
using UnityEngine.UI;

public class LandPurchaseUI : MonoBehaviour
{
    public LandManager landManager;
    public Vector3 landPosition;
    public float landSize = 10f;
    public float landPrice = 1f; // Price in ICP
    public Button purchaseButton;
    public Button buildButton;
    private int lastPurchasedLandId;

    void Start()
    {
        purchaseButton.onClick.AddListener(PurchaseLand);
        buildButton.onClick.AddListener(BuildStructure);
        buildButton.interactable = false;
    }

    async void PurchaseLand()
    {
        await landManager.PurchaseLand(landPosition, landSize, landPrice);
        lastPurchasedLandId = landManager.GetLastLandId(); // Simplified; in practice, parse from the response
        buildButton.interactable = true;
    }

    void BuildStructure()
    {
        landManager.BuildStructure(lastPurchasedLandId, "House");
    }
}
