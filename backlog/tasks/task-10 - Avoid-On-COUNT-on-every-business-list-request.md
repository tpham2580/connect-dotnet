---
id: TASK-10
title: Avoid O(n) COUNT(*) on every business list request
status: To Do
assignee: []
created_date: '2026-08-08 04:42'
labels:
  - backend
  - performance
  - api
  - tech-debt
dependencies:
  - TASK-1
ordinal: 10000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
GET /v1/businesses uses keyset pagination (TASK-1), so fetching a page no longer scans skipped rows. But the response still includes a 'total' field backed by 'SELECT COUNT(*) FROM business' in BusinessRepository.GetBusinessesAsync, which Postgres cannot answer in constant time: MVCC forces a full heap or index-only scan to establish row visibility. Every list request therefore pays an O(n) cost that grows with the table, negating much of the keyset benefit once the business table is large. Decide how 'total' should behave at scale and implement it. Options worth weighing: drop 'total' entirely (the usual cursor-pagination contract), make it opt-in via a query parameter so the common path stays cheap, serve an approximate count from pg_class.reltuples, or maintain a cached/materialized counter. Note this is a REST contract decision, not just an optimization, because 'total' is currently a required field on BusinessListResponse.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Listing businesses no longer performs an unconditional COUNT(*) per request
- [ ] #2 The behaviour of 'total' (removed, optional, approximate, or cached) is documented in the API contract
- [ ] #3 A test covers the chosen behaviour of 'total'
<!-- AC:END -->
