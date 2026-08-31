using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// AR 平面检测 + 标记放置 — 核心交互逻辑
/// </summary>
public class ARPlacementManager : MonoBehaviour
{
    [Header("AR 组件")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject markerPrefab;

    [Header("引用")]
    [SerializeField] private IssueReporter issueReporter;

    private static readonly List<ARRaycastHit> hits = new();

    void Update()
    {
        // 1. 仅处理单指触摸开始
        if (Input.touchCount == 0) return;
        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // 2. 防误触：触摸在 UI 元素上时忽略
        if (IsTouchOverUI(touch.position)) return;

        // 3. AR Raycast 检测水平平面
        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            // 4. 在命中点放置标记
            var hitPose = hits[0].pose;
            var marker = Instantiate(markerPrefab, hitPose.position, hitPose.rotation);

            // 5. 根据当前选择的优先级设置标记颜色
            SetMarkerColor(marker, issueReporter.CurrentPriority);

            // 6. 通知表单组件显示，传入位置坐标
            issueReporter.ShowForm(hitPose.position);
        }
    }

    /// <summary>
    /// 防误触检测：判断触摸点是否在 UI 元素上
    /// </summary>
    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    /// <summary>
    /// 根据优先级设置标记颜色
    /// </summary>
    private void SetMarkerColor(GameObject marker, string priority)
    {
        var renderer = marker.GetComponent<Renderer>();
        if (renderer == null) return;

        switch (priority)
        {
            case "high":
                renderer.material.color = Color.red;
                break;
            case "medium":
                renderer.material.color = Color.yellow;
                break;
            case "low":
                renderer.material.color = Color.green;
                break;
            default:
                renderer.material.color = Color.white;
                break;
        }
    }
}
