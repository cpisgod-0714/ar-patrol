using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 问题上报表单 — 填写并提交巡检问题到 Go 后端
/// </summary>
public class IssueReporter : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private GameObject formPanel;
    [SerializeField] private InputField titleInput;
    [SerializeField] private InputField descriptionInput;
    [SerializeField] private Dropdown priorityDropdown;
    [SerializeField] private Text statusText;

    [Header("API")]
    [SerializeField] private ApiClient apiClient;

    // 当前放置标记的位置
    private Vector3? markerPosition;

    // 当前选择的优先级（供 ARPlacementManager 读取）
    public string CurrentPriority => GetPriorityFromDropdown();

    void Awake()
    {
        // 初始化下拉选项
        priorityDropdown.ClearOptions();
        priorityDropdown.AddOptions(new List<string> { "low", "medium", "high" });
        priorityDropdown.value = 1; // 默认 medium

        HideForm();
    }

    /// <summary>
    /// 显示表单（由 ARPlacementManager 调用）
    /// </summary>
    public void ShowForm(Vector3 position)
    {
        markerPosition = position;
        formPanel.SetActive(true);
        statusText.text = "";
        statusText.color = Color.white;
    }

    /// <summary>
    /// 隐藏表单
    /// </summary>
    public void HideForm()
    {
        formPanel.SetActive(false);
        markerPosition = null;
    }

    /// <summary>
    /// 提交问题（绑定到提交按钮的 OnClick）
    /// </summary>
    public void OnSubmit()
    {
        // 校验标题
        if (string.IsNullOrWhiteSpace(titleInput.text))
        {
            statusText.text = "⚠️ 标题不能为空";
            statusText.color = Color.red;
            return;
        }

        if (!markerPosition.HasValue)
        {
            statusText.text = "⚠️ 位置信息丢失，请重新放置标记";
            statusText.color = Color.red;
            return;
        }

        // 构造请求体（字段名与数据契约一致）
        var request = new CreateIssueRequest
        {
            title = titleInput.text.Trim(),
            description = descriptionInput.text ?? "",
            priority = GetPriorityFromDropdown(),
            position = new PositionData(
                markerPosition.Value.x,
                markerPosition.Value.y,
                markerPosition.Value.z
            )
        };

        // 显示提交中状态
        statusText.text = "提交中...";
        statusText.color = Color.yellow;

        // 发送 API 请求
        apiClient.PostIssue(request, (success, response) =>
        {
            if (success)
            {
                statusText.text = "✅ 提交成功";
                statusText.color = Color.green;

                // 延迟关闭表单
                StartCoroutine(HideFormAfterDelay(1.5f));
            }
            else
            {
                statusText.text = $"❌ 提交失败: {response}";
                statusText.color = Color.red;
            }
        });
    }

    /// <summary>
    /// 取消（绑定到取消按钮的 OnClick）
    /// </summary>
    public void OnCancel()
    {
        HideForm();
    }

    private string GetPriorityFromDropdown()
    {
        string[] priorities = { "low", "medium", "high" };
        int idx = priorityDropdown.value;
        return priorities[Mathf.Clamp(idx, 0, 2)];
    }

    private System.Collections.IEnumerator HideFormAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // 清空输入
        titleInput.text = "";
        descriptionInput.text = "";
        HideForm();
    }
}
