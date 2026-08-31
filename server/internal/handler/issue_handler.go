package handler

import (
	"encoding/json"
	"net/http"

	"ar-patrol-server/internal/model"
	"ar-patrol-server/internal/store"
)

// IssueHandler 问题相关的 HTTP Handler
type IssueHandler struct {
	store *store.SQLiteStore
}

// NewIssueHandler 创建 Handler 实例
func NewIssueHandler(s *store.SQLiteStore) *IssueHandler {
	return &IssueHandler{store: s}
}

// HandleHealth 健康检查
func (h *IssueHandler) HandleHealth(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}

// HandleCreateIssue 创建问题
func (h *IssueHandler) HandleCreateIssue(w http.ResponseWriter, r *http.Request) {
	var req model.CreateIssueRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		writeError(w, http.StatusBadRequest, "invalid request body")
		return
	}

	if err := req.Validate(); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	issue := req.NewIssue()

	if err := h.store.Create(issue); err != nil {
		writeError(w, http.StatusInternalServerError, "failed to save issue")
		return
	}

	writeJSON(w, http.StatusCreated, issue)
}

// HandleListIssues 获取问题列表
func (h *IssueHandler) HandleListIssues(w http.ResponseWriter, r *http.Request) {
	issues, err := h.store.List()
	if err != nil {
		writeError(w, http.StatusInternalServerError, "failed to list issues")
		return
	}

	writeJSON(w, http.StatusOK, issues)
}

// HandleUpdateIssue 修改问题状态
func (h *IssueHandler) HandleUpdateIssue(w http.ResponseWriter, r *http.Request) {
	id := r.PathValue("id")
	if id == "" {
		writeError(w, http.StatusBadRequest, "missing issue id")
		return
	}

	var req model.UpdateIssueRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		writeError(w, http.StatusBadRequest, "invalid request body")
		return
	}

	if err := req.Validate(); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	issue, err := h.store.UpdateStatus(id, req.Status)
	if err == model.ErrIssueNotFound {
		writeError(w, http.StatusNotFound, err.Error())
		return
	}
	if err != nil {
		writeError(w, http.StatusInternalServerError, "failed to update issue")
		return
	}

	writeJSON(w, http.StatusOK, issue)
}

// --- 响应辅助函数 ---

func writeJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(data)
}

func writeError(w http.ResponseWriter, status int, message string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(map[string]string{"error": message})
}
