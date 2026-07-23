---
id: TASK-8
title: Handle NULL latitude/longitude in BusinessRepository reads
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
labels:
  - businessservice
  - bug
  - database
dependencies: []
priority: medium
ordinal: 8000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
init.sql declares latitude/longitude as nullable DOUBLE PRECISION, but GetBusinessByIdAsync and GetAllBusinessesByIdsAsync call reader.GetDouble unconditionally, throwing InvalidCastException for any row with NULL coordinates. Add NOT NULL to the schema or null-check the reader.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 GetBusinessById and GetAllBusinessesByIds do not throw on NULL latitude/longitude
- [ ] #2 Schema enforces NOT NULL coordinates or the model represents coordinates as nullable
- [ ] #3 A test covers a business row with NULL coordinates
<!-- AC:END -->
