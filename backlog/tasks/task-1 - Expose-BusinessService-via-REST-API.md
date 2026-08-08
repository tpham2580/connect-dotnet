---
id: TASK-1
title: Expose BusinessService via REST API
status: Done
assignee: []
created_date: '2026-07-23 01:58'
updated_date: '2026-08-08 05:30'
labels:
  - backend
  - api
dependencies: []
references:
  - 'https://github.com/tpham2580/connect-dotnet/pull/9'
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
- [x] #1 GET /v1/businesses returns paged results
- [x] #2 POST /v1/businesses validates payload and returns 201
- [x] #3 Integration tests cover happy path and 400/404
- [x] #4 GET /v1/businesses/{id} returns 200 with the business or 404 when unknown
- [x] #5 PUT /v1/businesses/{id} updates a business (200) and returns 404 for an unknown id
- [x] #6 DELETE /v1/businesses/{id} removes a business (204) and returns 404 for an unknown id
- [x] #7 Unit tests cover ListBusinesses paging guards and the RestAPI BusinessService gRPC mapping
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Extend TASK-1 to full CRUD + unit tests. 1) BusinessService(server): extract Application.IBusinessService (implemented by BusinessService), depend on it in BusinessGrpc, register in Program.cs DI; make UpdateBusiness return NotFound (not InvalidArgument) when the id is missing. 2) RestAPI: add UpdateAsync(id,req)/DeleteAsync(id) to IBusinessService+BusinessService wrapper (map DTO<->gRPC, RpcException NotFound -> null/false); BusinessController PUT /businesses/{id} (200/404, auto-400) + DELETE /businesses/{id} (204/404). 3) Tests: FakeBusinessServiceClient Update/Delete overrides; BusinessControllerTests PUT(200/404/400)+DELETE(204/404); new RestAPI.Tests/BusinessServiceTests (wrapper mapping + NotFound->null/false); new BusinessService.Tests BusinessGrpcTests (ListBusinesses limit/offset guards, Update/Delete NotFound) via hand-rolled IBusinessService fake. Validate: dotnet build + dotnet test.

Review follow-up (PR #9). 1) Consolidate duplicated protos into top-level /Protos; update BusinessService/LocationService/RestAPI csproj Protobuf includes with ProtoRoot=..; add COPY Protos/ to all three Dockerfiles. 2) Replace OFFSET paging with keyset paging: ListBusinessesRequest{limit,after}, ListBusinessesResponse{businesses,total,next_cursor,has_more}; repository selects WHERE business_id > @after ORDER BY business_id LIMIT n+1; REST contract becomes GET /v1/businesses?after&pageSize returning {pageSize,total,nextCursor,businesses}. 3) Guard NULL latitude/longitude in every BusinessRepository read via a shared helper. 4) Wrap page+COUNT(*) in a REPEATABLE READ transaction. 5) Map RpcException statuses to HTTP (InvalidArgument 400, NotFound 404, Unavailable/DeadlineExceeded 503, else 500 generic, no raw gRPC message leakage) and constrain routes to min(1). 6) Add ProducesResponseType to every action. 7) Nits: drop required on non-nullable value types, simplify response != false. 8) Update fakes/tests, add regression coverage, run full build+test.
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

User approved DTO structural refinement: convert TASK-1 Business request/response contracts to sealed immutable classes with required init-only properties, and expose paged businesses as IReadOnlyList. This intentionally avoids record-generated value semantics because records do not reduce heap allocation for these reference DTOs.

Implemented immutable Business DTO contracts: sealed classes, required init-only properties, and IReadOnlyList for paged response items. Updated integration test builders to construct invalid/updated requests without post-construction mutation. RestAPI.Tests: 18 passed, 0 failed.

Review decision: /v1/businesses is the intended versioned API contract; corrected acceptance-criteria route wording. Paging hardening plan: reject page/pageSize outside supported ranges with automatic HTTP 400 validation, reject combinations whose gRPC int32 offset would overflow, retain defensive service-layer guards, and add HTTP plus unit regression coverage.

Addressed review findings: paging now returns 400 for page/pageSize outside supported ranges and for combinations exceeding the gRPC int32 offset; the RestAPI wrapper also guards direct callers. Cancellation now flows from ASP.NET through the RestAPI gRPC client, BusinessService ServerCallContext/application layer, and every Npgsql async operation. Added HTTP, wrapper, and gRPC propagation regression tests. Validation: solution build succeeded; 39 tests passed (RestAPI.Tests 27, BusinessService.Tests 12), 0 failed.

Addressed PR #9 review (human-directed, all items approved incl. keyset paging and full proto consolidation).
- Protos: business.proto and location.proto deduplicated into a top-level /Protos folder; BusinessService/LocationService/RestAPI csproj now use Include=..\Protos\x.proto with ProtoRoot=..; all three Dockerfiles COPY Protos/ before restore. Verified by replaying each Dockerfile's exact COPY set into a scratch context and building - all three succeed.
- Paging: replaced LIMIT/OFFSET with keyset paging. ListBusinessesRequest is now {limit, after}; ListBusinessesResponse adds next_cursor + has_more. Repository selects WHERE business_id > @after ORDER BY business_id LIMIT n+1 and trims the probe row. REST contract changed from ?page&pageSize to ?after&pageSize, returning {pageSize,total,hasMore,nextCursor,businesses}. The int32 offset-overflow guard is gone because it is no longer reachable.
- NULL coordinates: added a single MapBusiness(reader) helper used by every read path, so GetBusinessById/GetAllBusinessesByIds/Create/Update no longer throw InvalidCastException on rows with NULL latitude/longitude (previously only the list path was guarded).
- Consistency: page query and COUNT(*) now share one REPEATABLE READ transaction.
- Error mapping: new RestAPI/Infrastructure/RpcExceptionHandler (IExceptionHandler) maps downstream gRPC statuses to HTTP (InvalidArgument/FailedPrecondition/OutOfRange 400, Unauthenticated 401, PermissionDenied 403, NotFound 404, AlreadyExists/Aborted 409, ResourceExhausted 429, Unimplemented 501, Unavailable 503, DeadlineExceeded 504, else 500). Upstream detail is echoed only for 4xx; 5xx returns a generic title. Replaced the old /error endpoint that echoed raw exception messages; the handler now runs in every environment. Fixes PUT /v1/businesses/{id} returning 500 for ids rejected upstream.
- Routes constrained to {id:long:min(1)} so non-positive ids 404 instead of reaching the backend.
- Added [ProducesResponseType] to every action plus [Produces(application/json)] so the Swagger doc lists 200/201/204/400/404.
- Nit: simplified 'response != false' in Application.BusinessService.DeleteBusinessByIdAsync.
- Deliberately NOT changed: 'required' on BusinessModel.Latitude/Longitude was left in place; removing it would let a model be constructed with a silent 0,0 default, which is worse than the cosmetic concern raised in review.
Validation: solution builds with no new warnings; 53 tests pass (RestAPI.Tests 39, BusinessService.Tests 14), 0 failed.

AC verification (2026-08-07, branch @ 3479b8b, merged as b479fa3): `dotnet test MySolution.sln` -> 53/53 passed (BusinessService.Tests 14, RestAPI.Tests 39), 0 failed. Integration tests boot the API in-memory via WebApplicationFactory<Program> with the gRPC backend faked (RestAPI.Tests/BusinessApiFactory.cs).

- AC1 paged list (keyset): GetBusinesses_ReturnsFirstPage, GetBusinesses_WalksPagesUsingCursor, GetBusinesses_WithCursorPastTheEnd_ReturnsEmptyPage.
- AC2 POST 201 + validation: CreateBusiness_WithValidPayload_Returns201AndIsRetrievable (201 + Location + re-GET), CreateBusiness_WithInvalidPayload_Returns400.
- AC3 happy/400/404: 400 via GetBusinesses_WithInvalidPaging_Returns400 (pageSize 0/101, after=-1) and Create/Update_WithInvalidPayload_Returns400; 404 via GetBusinessById/UpdateBusiness/DeleteBusiness_WithUnknownId_Returns404 and Routes_WithNonPositiveId_Return404_InsteadOfServerError (GET/PUT/DELETE).
- AC4 GET by id: 404 via GetBusinessById_WithUnknownId_Returns404; 200 asserted inside CreateBusiness_WithValidPayload_Returns201AndIsRetrievable (BusinessControllerTests.cs:139) and UpdateBusiness_WithValidPayload_Returns200AndPersists.
- AC5 PUT: UpdateBusiness_WithValidPayload_Returns200AndPersists, UpdateBusiness_WithUnknownId_Returns404.
- AC6 DELETE: DeleteBusiness_WithExistingId_Returns204AndIsGone, DeleteBusiness_WithUnknownId_Returns404.
- AC7 unit coverage: BusinessGrpcTests ListBusinesses_{CapsLimit_AtMax,DefaultsLimit_WhenNonPositive,ClampsNegativeCursor_ToZero,SetsNextCursor_ToLastId_WhenMorePagesExist,LeavesNextCursorUnset_OnLastPage,PassesCursor_AndMapsTotal}; RestAPI BusinessServiceTests mapping (GetByIdAsync_KnownId_MapsAllFields, ListAsync_MapsCursorAndTotal, CreateAsync_ReturnsMappedResponseWithGeneratedId, Update/DeleteAsync NotFound -> null/false).

Note AC1's contract changed during the PR #9 review follow-up: offset paging (page/pageSize) was replaced by keyset paging (GET /v1/businesses?after&pageSize -> {pageSize,total,nextCursor,businesses}). The residual COUNT(*) cost of 'total' is tracked separately as TASK-10. CI run 31241424381 green on the merged head.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Exposed BusinessService over HTTP as a versioned REST API (RestAPI/Controllers/BusinessController.cs, route v1/businesses) backed by a typed gRPC client wrapper and immutable DTOs. Full CRUD: keyset-paginated list, GET by id, POST (201 + Location), PUT, DELETE, with paging guards (pageSize 1..100, non-negative cursor), non-positive route ids rejected as 404, and gRPC status codes mapped to HTTP (InvalidArgument 400, NotFound 404, PermissionDenied 403, Unavailable 503, DeadlineExceeded 504, else 500) without leaking upstream detail. Protos consolidated into a shared top-level /Protos. Verified with dotnet test MySolution.sln: 53/53 pass (RestAPI.Tests 39, BusinessService.Tests 14), covering every acceptance criterion by name; CI green. Merged via PR #9 as b479fa3. Follow-up tracked in TASK-10 (avoid O(n) COUNT(*) behind the 'total' field).
<!-- SECTION:FINAL_SUMMARY:END -->
