# 🏗️ 现场巡检问题标注系统

园区巡检人员用手机 AR 放置标记并上报问题，管理人员在网页中查看和更新状态。

## 技术栈

| 部分 | 技术 | 说明 |
|---|---|---|
| 移动端 | Unity LTS + C# + AR Foundation | 检测平面、放置标记、上报问题 |
| 管理端 | React + TypeScript + Vite | 查看问题、修改状态 |
| 后端 | Go + SQLite | 提供 API，数据持久化 |

## 项目结构

```
├── server/          # Go 后端
├── web/             # React 管理端
├── unity-ar/        # Unity AR 移动端
├── README.md        # 本文件
├── AI_LOG.md        # AI 使用记录
└── .gitignore
```

## 启动方式

### 1. Go 后端

```bash
cd server
go run ./cmd/server/
# 服务启动在 http://localhost:8080
```

### 2. React 管理端

```bash
cd web
npm install
npm run dev
# 开发服务器启动在 http://localhost:5173
# API 请求自动代理到 Go 后端
```

### 3. Unity AR 移动端

详见 [unity-ar/README.md](unity-ar/README.md)

项目已在 Unity Editor 中创建，场景和脚本已配置完成。真机构建需 Mac (iOS) 或直接 Build APK (Android)。

### Unity 真机配置 Go 后端地址

在 ApiClient 组件的 Inspector 中修改 `Server Url`：
- 模拟器：`http://localhost:8080/api`
- 真机：`http://<电脑局域网IP>:8080/api`（手机和电脑需同一 WiFi，电脑防火墙放行 8080 端口）

## API 接口

| 方法 | 路径 | 说明 |
|---|---|---|
| GET | /api/health | 健康检查 |
| POST | /api/issues | 创建问题 |
| GET | /api/issues | 获取问题列表 |
| PATCH | /api/issues/:id | 修改问题状态 |

### 创建问题请求体

```json
{
  "title": "入口墙面破损",
  "description": "左侧墙体存在裂缝",
  "priority": "high",
  "position": { "x": 0.42, "y": 0.03, "z": 1.26 }
}
```

### 修改状态请求体

```json
{ "status": "in_progress" }
```

## 使用版本

- Go 1.27.0
- Node.js 24.16.0
- npm 11.13.0
- Unity 2022.3 LTS
- AR Foundation 5.x
- React 19 + TypeScript 5.x + Vite 8.x

## 已完成

- [x] Go 后端：健康检查、创建问题、列表查询、修改状态、参数校验、SQLite 持久化、CORS
- [x] Go 后端：数据重启后仍存在（SQLite IF NOT EXISTS 建表）
- [x] Go 后端：参数校验（标题必填、枚举值校验、404 未找到）
- [x] React 管理端：问题列表展示（标题、优先级、状态、描述、时间、位置）
- [x] React 管理端：状态下拉框修改（open / in_progress / resolved）
- [x] React 管理端：后端不可用时显示错误提示 + 重试按钮（不白屏）
- [x] React 管理端：空列表友好提示
- [x] React 管理端：Vite 代理配置 /api → localhost:8080
- [x] Unity AR：C# 脚本（ARPlacementManager、IssueReporter、ApiClient、IssueData）
- [x] Unity AR：场景搭建（XR Origin + AR Camera + Plane Manager + Raycast Manager + EventSystem）
- [x] Unity AR：ARMarker 预制体 + IssueForm UI Panel（TMP）
- [x] Unity AR：脚本挂载 + 引用绑定 + 按钮事件绑定
- [x] Unity AR：UI 防误触（IsTouchOverUI）
- [x] Unity AR：优先级颜色标记（high=红, medium=黄, low=绿）
- [x] Unity AR：TMP 组件适配（TMP_InputField / TMP_Dropdown / TextMeshProUGUI）
- [x] Unity Editor 内编译和 UI 交互测试通过

## 未完成

- [ ] 真机 AR 测试（需 Mac 构建 iOS 或 Android 设备）
- [ ] 演示视频（手机 + 电脑各一段）
- [ ] gh CLI 安装 + GitHub 推送

## 已知问题

- Go 后端 ID 生成使用时间戳+毫秒，高并发下可能重复（原型阶段可接受）
- iOS 构建需要 Mac + Xcode，当前开发环境为 Windows
- 修改状态后 updatedAt 时间戳与 createdAt 相同（毫秒精度不足，需确认）
