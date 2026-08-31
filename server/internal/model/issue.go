package model

import (
	"fmt"
	"time"
)

// Issue 巡检问题 — 字段名与数据契约严格一致
type Issue struct {
	ID          string   `json:"id"`
	Title       string   `json:"title"`
	Description string   `json:"description"`
	Priority    string   `json:"priority"`
	Status      string   `json:"status"`
	Position    Position `json:"position"`
	CreatedAt   string   `json:"createdAt"`
	UpdatedAt   string   `json:"updatedAt"`
}

// Position AR 标记位置坐标
type Position struct {
	X float64 `json:"x"`
	Y float64 `json:"y"`
	Z float64 `json:"z"`
}

// CreateIssueRequest 创建问题的请求体
type CreateIssueRequest struct {
	Title       string   `json:"title"`
	Description string   `json:"description"`
	Priority    string   `json:"priority"`
	Position    Position `json:"position"`
}

// UpdateIssueRequest 修改问题状态的请求体
type UpdateIssueRequest struct {
	Status string `json:"status"`
}

// 枚举校验
var validPriorities = map[string]bool{"low": true, "medium": true, "high": true}
var validStatuses = map[string]bool{"open": true, "in_progress": true, "resolved": true}

// ValidateCreate 校验创建请求
func (r *CreateIssueRequest) Validate() error {
	if r.Title == "" {
		return ErrTitleRequired
	}
	if !validPriorities[r.Priority] {
		return ErrInvalidPriority
	}
	return nil
}

// ValidateUpdate 校验更新请求
func (r *UpdateIssueRequest) Validate() error {
	if !validStatuses[r.Status] {
		return ErrInvalidStatus
	}
	return nil
}

// NewIssue 从创建请求构造完整 Issue（生成 id、默认状态、时间戳）
func (r *CreateIssueRequest) NewIssue() *Issue {
	now := time.Now().UTC().Format(time.RFC3339)
	return &Issue{
		ID:          fmtIssueID(),
		Title:       r.Title,
		Description: r.Description,
		Priority:    r.Priority,
		Status:      "open",
		Position:    r.Position,
		CreatedAt:   now,
		UpdatedAt:   now,
	}
}

func fmtIssueID() string {
	return "issue_" + time.Now().Format("20060102150405") + fmtMillis()
}

func fmtMillis() string {
	return fmt.Sprintf("%03d", time.Now().Nanosecond()/1e6)
}
