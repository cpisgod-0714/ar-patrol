package store

import (
	"database/sql"
	"fmt"
	"time"

	"ar-patrol-server/internal/model"

	_ "modernc.org/sqlite"
)

// SQLiteStore 基于 SQLite 的问题持久化
type SQLiteStore struct {
	db *sql.DB
}

// NewSQLiteStore 打开数据库并建表（IF NOT EXISTS 保证数据跨重启保留）
func NewSQLiteStore(dbPath string) (*SQLiteStore, error) {
	db, err := sql.Open("sqlite", dbPath)
	if err != nil {
		return nil, fmt.Errorf("open db: %w", err)
	}

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
		db.Close()
		return nil, fmt.Errorf("create table: %w", err)
	}

	return &SQLiteStore{db: db}, nil
}

// Close 关闭数据库连接
func (s *SQLiteStore) Close() error {
	return s.db.Close()
}

// Create 保存新问题
func (s *SQLiteStore) Create(issue *model.Issue) error {
	_, err := s.db.Exec(
		`INSERT INTO issues (id, title, description, priority, status, pos_x, pos_y, pos_z, created_at, updated_at)
		 VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
		issue.ID, issue.Title, issue.Description, issue.Priority, issue.Status,
		issue.Position.X, issue.Position.Y, issue.Position.Z,
		issue.CreatedAt, issue.UpdatedAt,
	)
	if err != nil {
		return fmt.Errorf("insert issue: %w", err)
	}
	return nil
}

// List 返回所有问题（按创建时间降序）
func (s *SQLiteStore) List() ([]model.Issue, error) {
	rows, err := s.db.Query(
		`SELECT id, title, description, priority, status, pos_x, pos_y, pos_z, created_at, updated_at
		 FROM issues ORDER BY created_at DESC`,
	)
	if err != nil {
		return nil, fmt.Errorf("query issues: %w", err)
	}
	defer rows.Close()

	var issues []model.Issue
	for rows.Next() {
		var i model.Issue
		err := rows.Scan(
			&i.ID, &i.Title, &i.Description, &i.Priority, &i.Status,
			&i.Position.X, &i.Position.Y, &i.Position.Z,
			&i.CreatedAt, &i.UpdatedAt,
		)
		if err != nil {
			return nil, fmt.Errorf("scan issue: %w", err)
		}
		issues = append(issues, i)
	}

	// 空列表返回 [] 而非 null
	if issues == nil {
		issues = []model.Issue{}
	}

	return issues, nil
}

// UpdateStatus 修改问题状态
func (s *SQLiteStore) UpdateStatus(id string, status string) (*model.Issue, error) {
	// 先检查是否存在
	var existing model.Issue
	err := s.db.QueryRow(
		`SELECT id, title, description, priority, status, pos_x, pos_y, pos_z, created_at, updated_at
		 FROM issues WHERE id = ?`, id,
	).Scan(
		&existing.ID, &existing.Title, &existing.Description, &existing.Priority, &existing.Status,
		&existing.Position.X, &existing.Position.Y, &existing.Position.Z,
		&existing.CreatedAt, &existing.UpdatedAt,
	)
	if err == sql.ErrNoRows {
		return nil, model.ErrIssueNotFound
	}
	if err != nil {
		return nil, fmt.Errorf("query issue: %w", err)
	}

	// 更新状态和时间戳
	updatedAt := time.Now().UTC().Format(time.RFC3339)
	_, err = s.db.Exec(
		`UPDATE issues SET status = ?, updated_at = ? WHERE id = ?`,
		status, updatedAt, id,
	)
	if err != nil {
		return nil, fmt.Errorf("update issue: %w", err)
	}

	existing.Status = status
	existing.UpdatedAt = updatedAt
	return &existing, nil
}
