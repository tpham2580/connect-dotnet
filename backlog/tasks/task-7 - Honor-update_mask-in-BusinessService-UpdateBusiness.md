---
id: TASK-7
title: Honor update_mask in BusinessService UpdateBusiness
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
labels:
  - businessservice
  - api
dependencies: []
priority: medium
ordinal: 7000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
The proto and README advertise partial updates via FieldMask, but UpdateBusinessAsync always overwrites every column, causing silent data loss for callers that send a partial update_mask. Either apply the field mask or remove it from the API and docs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Only fields present in update_mask are written to Postgres
- [ ] #2 Fields omitted from the mask retain their existing values
- [ ] #3 update_mask behavior is documented in the BusinessService README (or update_mask is removed from proto/README)
<!-- AC:END -->
