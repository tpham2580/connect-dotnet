---
id: TASK-2
title: Add Redis caching for business names
status: Done
assignee: []
created_date: '2026-07-23 01:58'
updated_date: '2026-08-08 05:09'
labels:
  - redis
  - performance
  - wontfix
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

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Closed without implementation. The premise does not match the system as built: nothing queries Postgres for business names on any read path, so there is no DB load to cut. LocationService.RedisLocationRepository already serves names directly from Redis business:{id} keys populated by redis/scripts/seed-redis.sh, and never contacts BusinessService or Postgres. AC #1 ('cache hit avoids DB query') therefore guards a query that does not exist, and the task appears to have been written against an assumed architecture rather than the implemented one.

It also conflicts with the direction now recorded on TASK-9. A TTL'd name cache alongside an authoritative geo index in the same Redis creates two different staleness models over the same data; a TTL that expires business:{id} names but not biz:geo members would yield nearby-search results carrying distances with missing names.

The genuine underlying question - where names come from on the nearby-search path - has been folded into TASK-9 as an explicit acceptance criterion. If the composition approach there ever proves too slow, caching should be reopened as a fresh, evidence-driven task with a measured hot path rather than an assumed one.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Closed as obsolete without implementation; acceptance criteria intentionally left unchecked. The task assumed Postgres-backed name lookups that do not exist - LocationService reads names straight from Redis and never touches Postgres - so there is no DB load to cache away. The real question of where nearby-search names should come from moved to TASK-9 (AC #4).
<!-- SECTION:FINAL_SUMMARY:END -->
