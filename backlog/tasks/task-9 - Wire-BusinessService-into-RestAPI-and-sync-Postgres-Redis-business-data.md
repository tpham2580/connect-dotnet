---
id: TASK-9
title: Wire BusinessService into RestAPI and sync Postgres/Redis business data
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
updated_date: '2026-08-08 04:43'
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

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
AC #1 satisfied by TASK-1 / PR #9 (branch restapi-businessservice, commit 327c4d5): RestAPI/Program.cs now registers Grpc.BusinessService.BusinessServiceClient via AddGrpcClient using GrpcSettings:BusinessServiceUrl, with the URL supplied in appsettings.Development.json and docker-compose (businessservice:6001). Full CRUD is exposed at /v1/businesses.

AC #2 and #3 remain open, and PR #9 makes them more urgent rather than less: the REST CRUD path writes only to Postgres, while LocationService reads exclusively from Redis (RedisLocationRepository, GEOSEARCH over the biz:geo geo set plus business:{id} name keys). The only writer to Redis is redis/scripts/seed-redis.sh, which hardcodes two businesses. Concrete drift now reachable through the public API: POST /v1/businesses creates a row that nearby-search can never return; DELETE /v1/businesses/{id} leaves a phantom entry in biz:geo whose name key still resolves; PUT changes to name or coordinates are invisible to LocationService. Worth deciding the ownership model before picking a mechanism - dual-write from BusinessService, a transactional outbox, or having LocationService treat BusinessService as the source of truth and cache from it (note TASK-2 proposes Redis caching of business names, which overlaps and should be reconciled with whatever is chosen here).
<!-- SECTION:NOTES:END -->
