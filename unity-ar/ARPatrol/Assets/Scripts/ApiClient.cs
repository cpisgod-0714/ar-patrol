using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Go 后端 API 客户端 — 封装所有 HTTP 请求
/// </summary>
public class ApiClient : MonoBehaviour
{
    [Header("后端地址（真机改为局域网 IP）")]
    [SerializeField] private string serverUrl = "http://192.168.1.100:8080/api";

    /// <summary>
    /// 提交巡检问题到后端
    /// </summary>
    public void PostIssue(CreateIssueRequest request, Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(request);
        StartCoroutine(PostRequest($"{serverUrl}/issues", json, callback));
    }

    /// <summary>
    /// 检查后端健康状态
    /// </summary>
    public void CheckHealth(Action<bool> callback)
    {
        StartCoroutine(GetRequest($"{serverUrl}/health", (ok, _) => callback(ok)));
    }

    private IEnumerator PostRequest(string url, string json, Action<bool, string> callback)
    {
        using var request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

#if UNITY_2022_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.Success)
#else
        if (!request.isNetworkError && !request.isHttpError)
#endif
        {
            callback(true, request.downloadHandler.text);
        }
        else
        {
            string errorMsg = TryExtractError(request.downloadHandler.text) ?? request.error;
            callback(false, errorMsg);
        }
    }

    private IEnumerator GetRequest(string url, Action<bool, string> callback)
    {
        using var request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

#if UNITY_2022_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.Success)
#else
        if (!request.isNetworkError && !request.isHttpError)
#endif
        {
            callback(true, request.downloadHandler.text);
        }
        else
        {
            callback(false, request.error);
        }
    }

    /// <summary>
    /// 尝试从后端错误响应中提取 error 字段
    /// </summary>
    private string TryExtractError(string responseBody)
    {
        if (string.IsNullOrEmpty(responseBody)) return null;
        try
        {
            var err = JsonUtility.FromJson<ApiErrorResponse>(responseBody);
            return string.IsNullOrEmpty(err?.error) ? null : err.error;
        }
        catch
        {
            return null;
        }
    }
}
