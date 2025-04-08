using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

public class ICClient : MonoBehaviour
{
    public async Task<string> CallCanister(string method, params object[] args)
    {
        string url = $"{BackendConfig.ICGateway}/api/v2/canister/{BackendConfig.CanisterId}/call";
        string body = JsonUtility.ToJson(new
        {
            method_name = method,
            args = args,
            sender = BackendConfig.PlayerId
        });

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error calling canister: {request.error}");
            return null;
        }

        return request.downloadHandler.text;
    }

    public async Task<string> QueryCanister(string method, params object[] args)
    {
        string url = $"{BackendConfig.ICGateway}/api/v2/canister/{BackendConfig.CanisterId}/query";
        string body = JsonUtility.ToJson(new
        {
            method_name = method,
            args = args,
            sender = BackendConfig.PlayerId
        });

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error querying canister: {request.error}");
            return null;
        }

        return request.downloadHandler.text;
    }
}
