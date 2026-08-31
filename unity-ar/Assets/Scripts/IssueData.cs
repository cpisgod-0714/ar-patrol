using UnityEngine;

/// <summary>
/// 巡检问题数据结构 — 字段名与后端数据契约严格一致
/// </summary>
[System.Serializable]
public class IssueData
{
    public string id;
    public string title;
    public string description;
    public string priority;  // low, medium, high
    public string status;    // open, in_progress, resolved
    public PositionData position;
    public string createdAt;
    public string updatedAt;
}

[System.Serializable]
public class PositionData
{
    public float x;
    public float y;
    public float z;

    public PositionData(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/// <summary>
/// 创建问题的请求体（不含后端生成的字段）
/// </summary>
[System.Serializable]
public class CreateIssueRequest
{
    public string title;
    public string description;
    public string priority;
    public PositionData position;
}

/// <summary>
/// API 响应包装（用于错误提示）
/// </summary>
[System.Serializable]
public class ApiErrorResponse
{
    public string error;
}
