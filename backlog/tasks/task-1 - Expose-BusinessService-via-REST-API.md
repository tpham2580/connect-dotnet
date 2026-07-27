---
id: TASK-1
title: Expose BusinessService via REST API
status: In Progress
assignee: []
created_date: '2026-07-23 01:58'
updated_date: '2026-07-27 02:28'
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
- [ ] #4 GET /businesses/{id} returns 200 with the business or 404 when unknown
- [ ] #5 PUT /businesses/{id} updates a business (200) and returns 404 for an unknown id
- [ ] #6 DELETE /businesses/{id} removes a business (204) and returns 404 for an unknown id
- [ ] #7 Unit tests cover ListBusinesses paging guards and the RestAPI BusinessService gRPC mapping
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Extend TASK-1 to full CRUD + unit tests. 1) BusinessService(server): extract Application.IBusinessService (implemented by BusinessService), depend on it in BusinessGrpc, register in Program.cs DI; make UpdateBusiness return NotFound (not InvalidArgument) when the id is missing. 2) RestAPI: add UpdateAsync(id,req)/DeleteAsync(id) to IBusinessService+BusinessService wrapper (map DTO<->gRPC, RpcException NotFound -> null/false); BusinessController PUT /businesses/{id} (200/404, auto-400) + DELETE /businesses/{id} (204/404). 3) Tests: FakeBusinessServiceClient Update/Delete overrides; BusinessControllerTests PUT(200/404/400)+DELETE(204/404); new RestAPI.Tests/BusinessServiceTests (wrapper mapping + NotFound->null/false); new BusinessService.Tests BusinessGrpcTests (ListBusinesses limit/offset guards, Update/Delete NotFound) via hand-rolled IBusinessService fake. Validate: dotnet build + dotnet test.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented end-to-end in worktree restapi-businessservice:
- BusinessService: added ListBusinesses RPC (business.proto) + BusinessRepository.GetBusinessesAsync (paged SELECT ORDER BY business_id LIMIT/OFFSET + COUNT(*) total), Application + BusinessGrpc handler with limit/offset guards (default/max 100, offset>=0).
- RestAPI: copied business.proto (GrpcServices=Client), registered BusinessServiceClient via GrpcSettings:BusinessServiceUrl in Program.cs, added BusinessServiceUrl to appsettings.Development.json + docker-compose restapi env/depends_on. Added IBusinessService/BusinessService gRPC-mapping wrapper, Business DTOs (DataAnnotations), BusinessController: GET /businesses?page&pageSize (200 paged), POST /businesses (201+Location / auto-400), GET /businesses/{id} (200/404). Exposed public partial class Program.
- Tests: replaced UnitTest1 with WebApplicationFactory<Program> integration tests using a hand-rolled FakeBusinessServiceClient (no live backend); 5 tests cover happy path (list+paging+POST 201+GET 200), 400 invalid POST, 404 unknown id.
- Housekeeping: reverted stray docker-compose postgres 5432->5433.
Validation: dotnet build (solution) succeeds; dotnet test = 10 passed (RestAPI.Tests 5, BusinessService.Tests 5), 0 failed. No new build warnings. Committed locally; not pushed, PR untouched, task left In Progress for human review.

Extended to full CRUD + unit tests (human-directed). Server: extracted Application.IBusinessService (BusinessGrpc now depends on it; registered in Program.cs DI); UpdateBusiness now returns NotFound (was InvalidArgument) when the id is missing. RestAPI: added UpdateAsync(id,req)/DeleteAsync(id) to IBusinessService+BusinessService (DTO<->gRPC map, RpcException NotFound -> null/false); BusinessController PUT /businesses/{id} (200/404, auto-400) + DELETE /businesses/{id} (204/404). Tests: FakeBusinessServiceClient Update/Delete overrides; +5 BusinessController CRUD tests; new BusinessServiceTests (8 wrapper mapping/RpcException tests); new BusinessGrpcTests (6 tests: ListBusinesses limit default/cap, negative-offset clamp, total mapping, Update/Delete NotFound) via hand-rolled FakeBusinessAppService. Validation: dotnet build (0 warnings introduced) + dotnet test = 29 passed (RestAPI.Tests 18, BusinessService.Tests 11), 0 failed.
<!-- SECTION:NOTES:END -->
