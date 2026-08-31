# Unity AR 移动端

## 状态

⚠️ Unity 项目尚未通过 Editor 创建。当前仅包含 C# 脚本文件，需在 Unity Editor 中完成以下步骤后才能使用。

## 配置步骤

1. 安装 Unity LTS (推荐 2022.3 或更高)
2. 在 Unity Hub 中新建 3D (URP) 项目
3. 通过 Package Manager 安装 AR Foundation
4. 在 XR Plug-in Management 中勾选 ARCore (Android) 或 ARKit (iOS)
5. 将本目录下的 `Assets/Scripts/` 复制到 Unity 项目中
6. 按以下结构配置场景组件：

### 场景结构

```
AR Session Origin
├── AR Camera (带 AR Camera Background + Tracked Pose Driver)
├── AR Plane Manager (Detection Mode = Horizontal)
├── AR Raycast Manager
└── ARPlacementManager (脚本)
```

### 必须的 GameObject

- **EventSystem** — UI 防误触依赖此组件
- **ARMarker Prefab** — 简单立方体/球体，Scale 约 (0.1, 0.1, 0.1)
- **IssueForm Panel** — 包含 Title InputField, Description InputField, Priority Dropdown, Submit/Cancel Button

### 后端地址配置

在 ApiClient 组件的 Inspector 中修改 `Server Url`：
- 模拟器: `http://localhost:8080/api`
- 真机: `http://<电脑局域网IP>:8080/api`

确保手机和电脑在同一局域网，电脑防火墙放行 8080 端口。

## 真机构建

### Android
- Build Settings → Android → Switch Platform
- Minimum API Level: 24 (Android 7.0)
- 需安装 Google Play Services for AR

### iOS
- 需要 Mac + Xcode
- 最低 iOS 13.0
- 需要 Apple Developer 账号签名
