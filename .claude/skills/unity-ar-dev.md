---
name: unity-ar-dev
description: Unity AR Foundation 移动端开发规范，涵盖项目结构、平面检测、标记放置、UI 防误触、真机调试
---

# Unity AR 移动端开发规范

## 1. 项目结构

```
Assets/
├── AR/                    # AR 相关场景和配置
│   └── Scenes/
│       └── MainScene.unity
├── Scripts/               # C# 脚本
│   ├── ARPlacementManager.cs   # 平面检测 + 标记放置
│   ├── IssueReporter.cs        # 问题上报表单 + API 调用
│   ├── IssueData.cs            # 数据结构（与后端一致）
│   └── ApiClient.cs            # HTTP 请求封装
├── Prefabs/               # 预制体
│   └── ARMarker.prefab         # 标记预制体（简单立方体/球体）
├── UI/                    # UI 元素
│   └── IssueForm.prefab        # 问题上报表单
└── Plugins/               # 平台相关插件（如需要）
```

## 2. AR Session 配置

场景中必须包含的 AR 组件层级：

```
AR Session Origin
├── AR Camera               # 主相机，带 AR Camera Background
├── AR Plane Manager        # 检测水平平面
│   └── Plane Prefab        # 平面可视化预制体
└── AR Placement Manager    # 自定义脚本，处理交互
```

**关键设置：**
- AR Plane Manager → Detection Mode = **Horizontal**（仅检测水平面）
- AR Session → 启用 ARCore (Android) 或 ARKit (iOS)
- 确保 AR Camera 有 Tracked Pose Driver 组件

## 3. 标记放置流程

```
用户触摸屏幕
  → 检查是否点在 UI 上（防误触）
  → 如果不在 UI 上，执行 AR Raycast
  → Raycast 命中水平平面？
    → 是：在命中点实例化 ARMarker 预制体
    → 否：忽略
```

### ARPlacementManager.cs 核心逻辑

```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private IssueReporter issueReporter;

    private static readonly List<ARRaycastHit> hits = new();

    void Update()
    {
        // 1. 仅处理单指触摸
        if (Input.touchCount == 0) return;
        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // 2. 防误触：检查是否点在 UI 元素上
        if (IsTouchOverUI(touch.position)) return;

        // 3. AR Raycast 检测平面
        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            // 4. 在命中点放置标记
            var hitPose = hits[0].pose;
            var marker = Instantiate(markerPrefab, hitPose.position, hitPose.rotation);

            // 5. 通知表单显示，传入位置坐标
            issueReporter.ShowForm(hitPose.position);
        }
    }

    // 防误触核心方法
    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
```

## 4. UI 防误触

**这是评审硬指标。** 必须确保：
- 点击输入框、按钮、下拉框时，**不会**在 AR 场景中放置标记
- 使用 `EventSystem.current.IsPointerOverGameObject()` 或上述 Raycast 方案
- 表单打开时，可选择额外禁用 AR 交互

### 推荐方案

```csharp
// 方法一：EventSystem 快速检查（适合简单 UI）
if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
    return;

// 方法二：GraphicRaycast 精确检查（适合复杂 UI）
// 见上方 IsTouchOverUI 方法
```

## 5. 问题上报表单 (IssueReporter.cs)

表单字段必须与数据契约一致：

| UI 控件 | 对应字段 | 校验 |
|---|---|---|
| InputField (Title) | `title` | 必填，不能为空 |
| InputField (Description) | `description` | 可选 |
| Dropdown (Priority) | `priority` | low / medium / high |
| 隐藏字段 (Position) | `position` | 自动填充 x/y/z |

提交流程：
1. 校验 title 非空
2. 构造 JSON 请求体（字段名严格按契约）
3. POST 到 `/api/issues`
4. 成功：显示成功提示，关闭表单
5. 失败：显示错误信息，保留表单内容让用户重试

### ApiClient.cs 示例

```csharp
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    // 后端地址通过配置或常量设置
    private const string BASE_URL = "http://<后端IP>:8080/api";

    public void PostIssue(IssueData issue, System.Action<bool, string> callback)
    {
        string json = JsonUtility.ToJson(issue);
        StartCoroutine(PostRequest($"{BASE_URL}/issues", json, callback));
    }

    private System.Collections.IEnumerator PostRequest(string url, string json, System.Action<bool, string> callback)
    {
        using var request = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            callback(true, request.downloadHandler.text);
        else
            callback(false, request.error);
    }
}
```

## 6. 后端地址配置

真机无法用 localhost，需要在 Unity 中配置后端 IP：

**方案一：常量配置（最简单）**
```csharp
// 开发时改成你的电脑局域网 IP
private const string BASE_URL = "http://192.168.1.100:8080/api";
```

**方案二：Inspector 可配置**
```csharp
[SerializeField] private string serverUrl = "http://192.168.1.100:8080/api";
```

**确保手机和电脑在同一局域网，且电脑防火墙放行端口。**

## 7. 真机调试指南

### Android (ARCore)
- 最低 API Level：24 (Android 7.0)
- 在 Edit → Project Settings → XR Plug-in Management 中勾选 ARCore
- 构建：Build Settings → Android → Switch Platform → Build
- 真机需要安装 Google Play Services for AR

### iOS (ARKit)
- 最低 iOS 版本：13.0
- 在 XR Plug-in Management 中勾选 ARKit
- 需要 Mac + Xcode 构建
- 需要 Apple Developer 账号签名

### 常见问题排查

| 问题 | 原因 | 解决 |
|---|---|---|
| 平面不检测 | 未勾选 ARCore/ARKit 插件 | XR Plug-in Management 确认勾选 |
| 光照估计失败 | 场景无 AR Camera Background | 确保 AR Camera 有此组件 |
| 触摸不响应 | 缺少 AR Raycast Manager | 添加并赋值到脚本 |
| UI 防误触失效 | 缺少 EventSystem | 场景中添加 EventSystem |
| 网络请求失败 | 未配置 INTERNET 权限 | Android Manifest 添加权限 |
| 后端连不上 | IP 配置错或防火墙 | 确认同网段 + 放行端口 |

## 8. 标记设计

标记直接使用 Unity 基础对象，不考察美术效果：
- 推荐用 **立方体** 或 **球体**，Scale 约 (0.1, 0.1, 0.1)
- 可根据优先级改变颜色：low=绿色, medium=黄色, high=红色
- 标记可挂载脚本保存关联的 Issue ID，方便后续扩展
