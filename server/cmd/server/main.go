package main

import (
	"log"
	"net/http"

	"ar-patrol-server/internal/handler"
	"ar-patrol-server/internal/middleware"
	"ar-patrol-server/internal/store"
)

func main() {
	// 初始化 SQLite 存储
	s, err := store.NewSQLiteStore("./data.db")
	if err != nil {
		log.Fatalf("Failed to open store: %v", err)
	}
	defer s.Close()

	// 创建 Handler
	h := handler.NewIssueHandler(s)

	// 注册路由（Go 1.22+ 方法路由 + 路径参数）
	mux := http.NewServeMux()
	mux.HandleFunc("GET /api/health", h.HandleHealth)
	mux.HandleFunc("POST /api/issues", h.HandleCreateIssue)
	mux.HandleFunc("GET /api/issues", h.HandleListIssues)
	mux.HandleFunc("PATCH /api/issues/{id}", h.HandleUpdateIssue)

	// CORS 中间件
	handler := middleware.CORS(mux)

	// 启动服务
	addr := ":8080"
	log.Printf("AR Patrol server starting on %s", addr)
	if err := http.ListenAndServe(addr, handler); err != nil {
		log.Fatalf("Server failed: %v", err)
	}
}
