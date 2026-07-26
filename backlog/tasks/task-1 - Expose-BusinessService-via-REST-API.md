---
id: TASK-1
title: Expose BusinessService via REST API
status: In Progress
assignee: []
created_date: '2026-07-23 01:58'
updated_date: '2026-07-26 03:40'
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

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. BusinessService: add ListBusinesses(ListBusinessesRequest{int32 limit,int32 offset}) returns ListBusinessesResponse{repeated Business businesses,int64 total} to business.proto; implement end-to-end: BusinessRepository.GetBusinessesAsync (SELECT ... ORDER BY business_id LIMIT/OFFSET + COUNT(*) total), Application/BusinessService.cs, Services/BusinessGrpc.cs handler (guard limit/offset: default 100, max 100, offset>=0). 2. RestAPI gRPC client: copy business.proto into RestAPI/Protos (GrpcServices=Client, mirrors location.proto), register BusinessServiceClient in Program.cs via GrpcSettings:BusinessServiceUrl; add BusinessServiceUrl to appsettings.Development.json and docker-compose restapi env+depends_on. 3. REST surface mirroring Location*: Dtos (BusinessRequest w/ DataAnnotations, BusinessResponse, BusinessListResponse{Page,PageSize,Total,list}); IBusinessService/BusinessService wrapper mapping DTO<->gRPC; BusinessController: GET /businesses?page&pageSize->200 (AC#1), POST /businesses->201+Location / 400 (AC#2), GET /businesses/{id}->200/404 (AC#3 404). 4. Integration tests: replace UnitTest1 with WebApplicationFactory<Program> tests; add Microsoft.AspNetCore.Mvc.Testing (net8); replace BusinessServiceClient with hand-rolled fake subclass (no real BusinessService); cover happy path (list+POST 201+get 200), 400 invalid POST, 404 unknown id; expose public partial class Program. 5. Housekeeping: revert stray docker-compose postgres 5432->5433. Validate: dotnet build + dotnet test.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented end-to-end in worktree restapi-businessservice:
- BusinessService: added ListBusinesses RPC (business.proto) + BusinessRepository.GetBusinessesAsync (paged SELECT ORDER BY business_id LIMIT/OFFSET + COUNT(*) total), Application + BusinessGrpc handler with limit/offset guards (default/max 100, offset>=0).
- RestAPI: copied business.proto (GrpcServices=Client), registered BusinessServiceClient via GrpcSettings:BusinessServiceUrl in Program.cs, added BusinessServiceUrl to appsettings.Development.json + docker-compose restapi env/depends_on. Added IBusinessService/BusinessService gRPC-mapping wrapper, Business DTOs (DataAnnotations), BusinessController: GET /businesses?page&pageSize (200 paged), POST /businesses (201+Location / auto-400), GET /businesses/{id} (200/404). Exposed public partial class Program.
- Tests: replaced UnitTest1 with WebApplicationFactory<Program> integration tests using a hand-rolled FakeBusinessServiceClient (no live backend); 5 tests cover happy path (list+paging+POST 201+GET 200), 400 invalid POST, 404 unknown id.
- Housekeeping: reverted stray docker-compose postgres 5432->5433.
Validation: dotnet build (solution) succeeds; dotnet test = 10 passed (RestAPI.Tests 5, BusinessService.Tests 5), 0 failed. No new build warnings. Committed locally; not pushed, PR untouched, task left In Progress for human review.
<!-- SECTION:NOTES:END -->
