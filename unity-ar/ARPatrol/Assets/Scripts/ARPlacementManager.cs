using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// AR 平面检测 + 标记放置 — 核心交互逻辑
/// Editor 模式下用鼠标点击直接放置，真机走 AR Raycast
/// </summary>
public class ARPlacementManager : MonoBehaviour
{
    [Header("AR 组件")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject markerPrefab;

    [Header("引用")]
    [SerializeField] private IssueReporter issueReporter;

    [Header("Editor 模式")]
    [SerializeField] private bool editorMode = true;
    [SerializeField] private float editorPlaceY = 0f; // 放置高度

    private static readonly List<ARRaycastHit> hits = new();

    void Update()
    {
#if UNITY_EDITOR
        if (editorMode)
        {
            UpdateEditor();
            return;
        }
#endif
        UpdateAR();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor 模式：鼠标左键点击 Game 窗口 → 在点击位置放置标记
    /// </summary>
    void UpdateEditor()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (IsPointerOverUI(Input.mousePosition)) return;

        // 将屏幕坐标转为世界坐标：x/z 按屏幕比例映射，y 固定
        var screenPos = Input.mousePosition;
        var cam = Camera.main;
        if (cam == null) return;

        // 从相机发射射线，与 y=editorPlaceY 平面求交
        var ray = cam.ScreenPointToRay(screenPos);
        float t = (editorPlaceY - ray.origin.y) / ray.direction.y;
        if (t < 0) return; // 射线方向不对

        var worldPos = ray.origin + ray.direction * t;
        PlaceMarker(worldPos, Quaternion.identity);
    }
#endif

    /// <summary>
    /// 真机模式：触摸 + AR Raycast 检测水平平面
    /// </summary>
    void UpdateAR()
    {
        if (Input.touchCount == 0) return;
        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;
        if (IsPointerOverUI(touch.position)) return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            PlaceMarker(hitPose.position, hitPose.rotation);
        }
    }

    /// <summary>
    /// 放置标记 + 着色 + 弹出表单
    /// </summary>
    void PlaceMarker(Vector3 position, Quaternion rotation)
    {
        var marker = Instantiate(markerPrefab, position, rotation);
        SetMarkerColor(marker, issueReporter.CurrentPriority);
        issueReporter.ShowForm(position);
    }

    /// <summary>
    /// 防误触检测：判断触摸/鼠标点是否在 UI 元素上
    /// </summary>
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
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
