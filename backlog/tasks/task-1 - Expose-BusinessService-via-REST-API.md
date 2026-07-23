---
id: TASK-1
title: Expose BusinessService via REST API
status: In Progress
assignee: []
created_date: '2026-07-23 01:58'
updated_date: '2026-07-23 04:10'
labels:
  - backend
  - api
dependencies: []
references:
  - 'https://github.com/tpham2580/connect-dotnet/pull/42'
  - 'https://github.com/tpham2580/connect-dotnet/tree/restapi-businessservice'
priority: high
ordinal: 1000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add REST endpoints (CRUD) for BusinessService so external clients can manage businesses over HTTP.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 GET /businesses returns paged results
- [ ] #2 POST /businesses validates payload and returns 201
- [ ] #3 Integration tests cover happy path and 400/404
<!-- AC:END -->
