using UnityEngine;
using System.Collections.Generic;

public class CitadelSurveillance : MonoBehaviour
{
    public GameObject kodoEnforcerPrefab;
    private List<GameObject> activeEnforcers = new List<GameObject>();
    private GameObject player;
    private bool isPlayerInRestrictedArea = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (isPlayerInRestrictedArea && activeEnforcers.Count == 0)
        {
            Vector3 spawnPos = player.transform.position + new Vector3(10, 0, 10);
            GameObject enforcer = Instantiate(kodoEnforcerPrefab, spawnPos, Quaternion.identity);
            activeEnforcers.Add(enforcer);
            Debug.Log("Citadel has detected a crime! Kodo Enforcer dispatched.");
        }

        foreach (var enforcer in activeEnforcers)
        {
            if (enforcer != null)
            {
                enforcer.transform.position = Vector3.MoveTowards(enforcer.transform.position, player.transform.position, Time.deltaTime * 5f);
                if (Vector3.Distance(enforcer.transform.position, player.transform.position) < 1f)
                {
                    player.GetComponent<PlayerController>().TakeDamage(100f, "Kodo Enforcer");
                    Destroy(enforcer);
                    activeEnforcers.Remove(enforcer);
                    isPlayerInRestrictedArea = false;
                    break;
                }
            }
        }
    }

    public void PlayerEnteredRestrictedArea()
    {
        isPlayerInRestrictedArea = true;
    }

    public void PlayerExitedRestrictedArea()
    {
        isPlayerInRestrictedArea = false;
        foreach (var enforcer in activeEnforcers)
        {
            if (enforcer != null)
            {
                Destroy(enforcer);
            }
        }
        activeEnforcers.Clear();
        Debug.Log("Player left the restricted area. Kodo Enforcers retreating.");
    }
}