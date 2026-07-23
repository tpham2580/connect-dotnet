---
id: TASK-2
title: Add Redis caching for business names
status: To Do
assignee: []
created_date: '2026-07-23 01:58'
labels:
  - redis
  - performance
dependencies: []
priority: medium
ordinal: 2000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Cache business-name lookups in Redis to cut Postgres load on hot paths.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Cache hit avoids DB query
- [ ] #2 Keys use a versioned business: prefix
- [ ] #3 TTL is configurable via env
<!-- AC:END -->
