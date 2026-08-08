---
id: TASK-9
title: Wire BusinessService into RestAPI and sync Postgres/Redis business data
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
updated_date: '2026-08-08 05:10'
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
- [ ] #4 Nearby-search results source business names from a single owner, with the duplicated Redis business:{id} name keys either justified or eliminated
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
AC #1 satisfied by TASK-1 / PR #9 (branch restapi-businessservice, commit 327c4d5): RestAPI/Program.cs now registers Grpc.BusinessService.BusinessServiceClient via AddGrpcClient using GrpcSettings:BusinessServiceUrl, with the URL supplied in appsettings.Development.json and docker-compose (businessservice:6001). Full CRUD is exposed at /v1/businesses.

AC #2 and #3 remain open, and PR #9 makes them more urgent rather than less: the REST CRUD path writes only to Postgres, while LocationService reads exclusively from Redis (RedisLocationRepository, GEOSEARCH over the biz:geo geo set plus business:{id} name keys). The only writer to Redis is redis/scripts/seed-redis.sh, which hardcodes two businesses. Concrete drift now reachable through the public API: POST /v1/businesses creates a row that nearby-search can never return; DELETE /v1/businesses/{id} leaves a phantom entry in biz:geo whose name key still resolves; PUT changes to name or coordinates are invisible to LocationService. Worth deciding the ownership model before picking a mechanism - dual-write from BusinessService, a transactional outbox, or having LocationService treat BusinessService as the source of truth and cache from it (note TASK-2 proposes Redis caching of business names, which overlaps and should be reconciled with whatever is chosen here).

Design direction from review discussion (2026-08-07), recorded as context; the worker should still form its own plan on pickup.

Ownership: Redis biz:geo is LocationService's private storage, so BusinessService must not write to it directly - that is a shared-database anti-pattern that would prevent LocationService changing its storage independently. Recommendation is a PULL model: LocationService owns and populates its own index by reading from BusinessService on an interval, building into a scratch key and using Redis RENAME (atomic) to swap it over biz:geo so searches never observe a half-built index.

Rationale for pull over push: (1) the dependency points the right way - BusinessService is the source of truth and needs no knowledge of its consumers, so it needs zero changes; (2) it is self-healing, since every pass converges, whereas a push design needs a transactional outbox purely to be correct because a failed push is permanent silent drift; (3) deletes are handled naturally by rebuild-and-swap, while push-based deletes are the easiest to drop and hardest to notice; (4) BusinessService's write path stops depending on LocationService availability.

Why silent drift matters more than normal staleness here: biz:geo is an index, not a cache. A cache miss costs latency, but a member missing from the geo set makes the business invisible to GEOSEARCH with no error raised - a silent correctness bug. README permits non-real-time updates ('info does not need to be reflected in real-time'), so bounded staleness is acceptable, but non-convergence is not.

Scale path: start with a full rebuild per pass, which is trivially correct at current volume. If that becomes expensive, add an updated_at column plus a 'changed since' query for incremental sync - at which point soft-delete tombstones become necessary, since a changefeed cannot observe rows that no longer exist.

Name duplication (absorbed from TASK-2, now closed): Redis currently stores coordinates in biz:geo AND names in business:{id}. Only coordinates are structurally required; the names are duplicated from Postgres and are pure sync surface. BusinessService.GetAllBusinessesByIds already exists and is currently unused by RestAPI, which now holds both gRPC clients. Leading option is for LocationService to return ids plus distances and for RestAPI to hydrate names via GetAllBusinessesByIds, reducing Redis to a pure geo index and removing name propagation entirely. Trade-off to decide: one extra gRPC hop per nearby search versus permanently halving the sync surface.
<!-- SECTION:NOTES:END -->
