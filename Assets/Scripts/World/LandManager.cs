using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LandManager : MonoBehaviour
{
    public GameObject landPrefab; // Prefab for visualizing land (e.g., a flat square)
    public GameObject housePrefab; // Prefab for a house structure
    private Dictionary<int, GameObject> landObjects = new Dictionary<int, GameObject>();
    private ICClient icClient;

    void Start()
    {
        icClient = FindObjectOfType<ICClient>();
        LoadLands();
    }

    async void LoadLands()
    {
        string result = await icClient.QueryCanister("getAllLands");
        if (!string.IsNullOrEmpty(result))
        {
            // Parse the JSON response (simplified)
            var lands = JsonUtility.FromJson<LandList>(result);
            foreach (var land in lands.lands)
            {
                int landId = land.id;
                Vector3 position = new Vector3(land.position.x, land.position.y, land.position.z);
                float size = land.size;

                GameObject landObj = Instantiate(landPrefab, position, Quaternion.identity);
                landObj.transform.localScale = new Vector3(size, 0.1f, size);
                landObjects[landId] = landObj;

                if (!string.IsNullOrEmpty(land.structure))
                {
                    // Build the structure (e.g., a house)
                    GameObject structure = Instantiate(housePrefab, position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                    structure.transform.SetParent(landObj.transform, false);
                }
            }
        }
    }

    public async Task PurchaseLand(Vector3 position, float size, float price)
    {
        string result = await icClient.CallCanister("purchaseLand", position.x, position.y, position.z, size, price);
        if (!string.IsNullOrEmpty(result))
        {
            // Parse the land ID (simplified)
            var landData = JsonUtility.FromJson<LandData>(result);
            int landId = landData.id;

            GameObject landObj = Instantiate(landPrefab, position, Quaternion.identity);
            landObj.transform.localScale = new Vector3(size, 0.1f, size);
            landObjects[landId] = landObj;
        }
    }

    public async void BuildStructure(int landId, string structureType)
    {
        string result = await icClient.CallCanister("buildStructure", landId, structureType);
        if (!string.IsNullOrEmpty(result) && result.Contains("true"))
        {
            GameObject landObj = landObjects[landId];
            GameObject structure = Instantiate(housePrefab, landObj.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
            structure.transform.SetParent(landObj.transform, false);
        }
    }
}

[System.Serializable]
public class LandList
{
    public LandData[] lands;
}

[System.Serializable]
public class LandData
{
    public int id;
    public string owner;
    public Position position;
    public float size;
    public float price;
    public string structure;
}
