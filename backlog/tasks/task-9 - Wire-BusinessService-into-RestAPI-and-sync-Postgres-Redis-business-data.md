---
id: TASK-9
title: Wire BusinessService into RestAPI and sync Postgres/Redis business data
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
labels:
  - backend
  - architecture
  - tech-debt
dependencies: []
priority: medium
ordinal: 9000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
RestAPI only has a LocationService gRPC client, so there is no REST path to BusinessService CRUD. Business names live in both Postgres (BusinessService) and Redis (business:{id}, biz:geo), seeded independently with no synchronization, so nearby-search results can drift from the source of truth. Add a BusinessService gRPC client to RestAPI and a propagation/sync mechanism. Complements TASK-1.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 RestAPI has a registered BusinessService gRPC client
- [ ] #2 Business create/update/delete propagates name and geo data to Redis
- [ ] #3 Source-of-truth/ownership for business data is documented
<!-- AC:END -->
