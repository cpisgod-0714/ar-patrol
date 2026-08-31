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

⚠️ 需先安装 Unity Editor 并创建项目，再将脚本导入。

## API 接口

| 方法 | 路径 | 说明 |
|---|---|---|
| GET | /api/health | 健康检查 |
| POST | /api/issues | 创建问题 |
| GET | /api/issues | 获取问题列表 |
| PATCH | /api/issues/:id | 修改问题状态 |

## 使用版本

- Go 1.27.0
- Node.js 24.16.0
- Unity LTS 2022.3+ (待安装)
- AR Foundation 5.x+

## 已完成

- [x] Go 后端：健康检查、创建问题、列表查询、修改状态、参数校验、SQLite 持久化、CORS
- [x] React 管理端：问题列表、状态修改、错误容错、Vite 代理
- [x] Unity AR：C# 脚本（数据结构、API 客户端、AR 放置、问题上报）

## 未完成

- [ ] Unity 项目在 Editor 中创建和配置
- [ ] AR 场景搭建（AR Session、Plane Manager 等）
- [ ] UI 表单预制体
- [ ] 真机测试
- [ ] 演示视频

## 已知问题

- Unity 项目尚未通过 Editor 创建，脚本无法单独运行
- Go 后端 ID 生成使用时间戳+毫秒，高并发下可能重复（原型阶段可接受）
