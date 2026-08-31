package model

import "errors"

var (
	ErrTitleRequired   = errors.New("title is required")
	ErrInvalidPriority = errors.New("invalid priority: must be low, medium, or high")
	ErrInvalidStatus   = errors.New("invalid status: must be open, in_progress, or resolved")
	ErrIssueNotFound   = errors.New("issue not found")
)
