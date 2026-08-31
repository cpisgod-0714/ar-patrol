---
name: go-api-dev
description: Go 后端 API 开发规范，涵盖项目结构、路由设计、数据持久化、参数校验、CORS、错误处理
---

# Go 后端开发规范

## 1. 项目结构

```
server/
├── cmd/
│   └── server/
│       └── main.go            # 入口，启动 HTTP 服务
├── internal/
│   ├── handler/
│   │   └── issue_handler.go   # HTTP Handler（路由处理）
│   ├── model/
│   │   └── issue.go           # Issue 数据模型
│   ├── store/
│   │   └── sqlite_store.go    # SQLite 持久化层
│   └── middleware/
│       └── cors.go            # CORS 中间件
├── go.mod
├── go.sum
└── README.md
```

简单项目可以扁平化，但 handler / model / store 分层**必须保持**，便于测试和维护。

## 2. 依赖选择

| 需求 | 推荐方案 | 备选 |
|---|---|---|
| HTTP 路由 | `net/http` 标准库 | `chi` / `echo` |
| SQLite 驱动 | `modernc.org/sqlite`（纯 Go，无 CGO） | `mattn/go-sqlite3`（需 CGO） |
| JSON 处理 | `encoding/json` 标准库 | — |
| ID 生成 | `fmt.Sprintf("issue_%d", time.Now().UnixMilli())` | `github.com/google/uuid` |

**优先用标准库**，减少外部依赖。框架不限但需在 README 说明。

## 3. 数据模型 (internal/model/issue.go)

```go
package model

import "time"

type Issue struct {
    ID          string    `json:"id"`
    Title       string    `json:"title"`
    Description string    `json:"description"`
    Priority    string    `json:"priority"`
    Status      string    `json:"status"`
    Position    Position  `json:"position"`
    CreatedAt   string    `json:"createdAt"`
    UpdatedAt   string    `json:"updatedAt"`
}

type Position struct {
    X float64 `json:"x"`
    Y float64 `json:"y"`
    Z float64 `json:"z"`
}

type CreateIssueRequest struct {
    Title       string   `json:"title"`
    Description string   `json:"description"`
    Priority    string   `json:"priority"`
    Position    Position `json:"position"`
}

type UpdateIssueRequest struct {
    Status string `json:"status"`
}
```

**注意：** JSON tag 必须与数据契约完全一致（驼峰命名：`createdAt` 而非 `created_at`）。

## 4. 参数校验

### 必须校验的规则

```go
var validPriorities = map[string]bool{"low": true, "medium": true, "high": true}
var validStatuses   = map[string]bool{"open": true, "in_progress": true, "resolved": true}

// 创建问题时
func validateCreate(req CreateIssueRequest) error {
    if strings.TrimSpace(req.Title) == "" {
        return fmt.Errorf("title is required")
    }
    if !validPriorities[req.Priority] {
        return fmt.Errorf("invalid priority: must be low, medium, or high")
    }
    return nil
}

// 修改状态时
func validateUpdate(req UpdateIssueRequest) error {
    if !validStatuses[req.Status] {
        return fmt.Errorf("invalid status: must be open, in_progress, or resolved")
    }
    return nil
}
```

## 5. 路由设计

```go
mux := http.NewServeMux()
mux.HandleFunc("GET /api/health", handleHealth)
mux.HandleFunc("POST /api/issues", handleCreateIssue)
mux.HandleFunc("GET /api/issues", handleListIssues)
mux.HandleFunc("PATCH /api/issues/{id}", handleUpdateIssue)
```

Go 1.22+ 支持路径参数 `{id}` 和方法路由。如用更低版本，可用 `chi` 路由。

### Handler 签名模式

```go
func handleCreateIssue(w http.ResponseWriter, r *http.Request) {
    // 1. 解码 JSON 请求体
    var req CreateIssueRequest
    if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
        writeError(w, http.StatusBadRequest, "invalid request body")
        return
    }

    // 2. 参数校验
    if err := validateCreate(req); err != nil {
        writeError(w, http.StatusBadRequest, err.Error())
        return
    }

    // 3. 构造完整 Issue（生成 id, status, 时间戳）
    issue := model.Issue{
        ID:          fmt.Sprintf("issue_%d", time.Now().UnixMilli()),
        Title:       req.Title,
        Description: req.Description,
        Priority:    req.Priority,
        Status:      "open",
        Position:    req.Position,
        CreatedAt:   time.Now().UTC().Format(time.RFC3339),
        UpdatedAt:   time.Now().UTC().Format(time.RFC3339),
    }

    // 4. 持久化
    if err := store.Create(&issue); err != nil {
        writeError(w, http.StatusInternalServerError, "failed to save issue")
        return
    }

    // 5. 返回完整对象
    writeJSON(w, http.StatusCreated, issue)
}
```

## 6. 错误响应统一格式

```go
func writeError(w http.ResponseWriter, status int, message string) {
    w.Header().Set("Content-Type", "application/json")
    w.WriteHeader(status)
    json.NewEncoder(w).Encode(map[string]string{"error": message})
}

func writeJSON(w http.ResponseWriter, status int, data interface{}) {
    w.Header().Set("Content-Type", "application/json")
    w.WriteHeader(status)
    json.NewEncoder(w).Encode(data)
}
```

## 7. CORS 中间件

**必须配置**，否则 React 和 Unity 无法跨域访问：

```go
func corsMiddleware(next http.Handler) http.Handler {
    return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
        w.Header().Set("Access-Control-Allow-Origin", "*")
        w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PATCH, OPTIONS")
        w.Header().Set("Access-Control-Allow-Headers", "Content-Type")

        if r.Method == "OPTIONS" {
            w.WriteHeader(http.StatusNoContent) // 204
            return
        }

        next.ServeHTTP(w, r)
    })
}
```

在 main.go 中包裹：
```go
handler := corsMiddleware(mux)
log.Fatal(http.ListenAndServe(":8080", handler))
```

## 8. SQLite 持久化

### 表初始化

```go
func NewSQLiteStore(dbPath string) (*SQLiteStore, error) {
    db, err := sql.Open("sqlite", dbPath) // modernc.org/sqlite 驱动名
    if err != nil {
        return nil, err
    }

    // 建表（服务启动时自动执行，数据跨重启保留）
    _, err = db.Exec(`
        CREATE TABLE IF NOT EXISTS issues (
            id          TEXT PRIMARY KEY,
            title       TEXT NOT NULL,
            description TEXT DEFAULT '',
            priority    TEXT NOT NULL,
            status      TEXT NOT NULL DEFAULT 'open',
            pos_x       REAL NOT NULL,
            pos_y       REAL NOT NULL,
            pos_z       REAL NOT NULL,
            created_at  TEXT NOT NULL,
            updated_at  TEXT NOT NULL
        )
    `)
    if err != nil {
        return nil, err
    }

    return &SQLiteStore{db: db}, nil
}
```

### 关键要点

- `CREATE TABLE IF NOT EXISTS` — 首次启动建表，后续启动保留数据
- 数据库文件存为 `./data.db`，Go 服务重启后数据仍存在
- 位置坐标拆为 `pos_x, pos_y, pos_z` 三列存储
- 查询时重新组装为 JSON 的 `position` 对象
- 确保 `sql.DB` 在服务退出时 `db.Close()`

## 9. 服务启动 (cmd/server/main.go)

```go
package main

import (
    "log"
    "net/http"
)

func main() {
    store, err := store.NewSQLiteStore("./data.db")
    if err != nil {
        log.Fatal(err)
    }
    defer store.Close()

    mux := http.NewServeMux()
    // 注册路由...

    handler := corsMiddleware(mux)
    log.Println("Server starting on :8080")
    log.Fatal(http.ListenAndServe(":8080", handler))
}
```

## 10. 检查清单

- [ ] 字段 JSON tag 与数据契约一致（驼峰命名）
- [ ] title 为空返回 400 + 明确错误信息
- [ ] priority/status 枚举值校验
- [ ] 新问题自动生成 id 和 status=open
- [ ] CORS 中间件已配置
- [ ] OPTIONS 请求返回 204
- [ ] SQLite 建表用 IF NOT EXISTS
- [ ] 服务重启后数据仍存在
- [ ] 空列表返回 `[]` 而非 `null`
- [ ] 错误响应统一 `{ "error": "..." }` 格式
