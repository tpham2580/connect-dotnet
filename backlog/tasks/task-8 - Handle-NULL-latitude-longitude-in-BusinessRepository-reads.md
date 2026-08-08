---
id: TASK-8
title: Handle NULL latitude/longitude in BusinessRepository reads
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
updated_date: '2026-08-08 04:43'
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

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Partially addressed by TASK-1 / PR #9 (branch restapi-businessservice, commit 327c4d5). BusinessRepository now routes every read through a single MapBusiness(reader) helper that coalesces NULL latitude/longitude to 0, so GetBusinessByIdAsync, GetAllBusinessesByIdsAsync and the Create/Update RETURNING reads no longer throw InvalidCastException. That covers AC #1. Still open: AC #2 (init.sql still declares latitude/longitude as nullable DOUBLE PRECISION with no NOT NULL, and BusinessModel still exposes them as non-nullable double, so NULL is silently rendered as 0,0 rather than represented honestly) and AC #3 (no test exercises a row with NULL coordinates - the repository has no test coverage because it requires a live Postgres). Decide between adding NOT NULL to the schema and making the model coordinates nullable; the current coalesce-to-0 is a crash guard, not a correctness fix, and 0,0 is a real location in the Gulf of Guinea.
<!-- SECTION:NOTES:END -->
