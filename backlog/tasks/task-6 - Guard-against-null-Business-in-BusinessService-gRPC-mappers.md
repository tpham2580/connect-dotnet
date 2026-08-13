---
id: TASK-6
title: Guard against null Business in BusinessService gRPC mappers
status: Done
assignee:
  - '@copilot'
created_date: '2026-07-23 04:10'
updated_date: '2026-08-13 03:41'
labels:
  - businessservice
  - bug
dependencies: []
references:
  - 'https://github.com/tpham2580/connect-dotnet/pull/13'
priority: medium
ordinal: 6000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
proto3 singular message fields can be unset, but BusinessMapper dereferences request.Business.* with no null check in CreateBusiness/UpdateBusiness. A missing payload surfaces as a gRPC Unknown/NRE instead of InvalidArgument.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 CreateBusiness and UpdateBusiness return InvalidArgument when the business payload is missing
- [x] #2 BusinessMapper no longer throws NullReferenceException on a null Business
- [x] #3 Unit test covers the missing-business case
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add a request-payload guard at the top of BusinessGrpc.CreateBusiness and BusinessGrpc.UpdateBusiness that throws RpcException(InvalidArgument) when request.Business is null, placed BEFORE BusinessMapper.ToBusinessModel so the mapper never sees unvalidated input (validate-then-map ordering is the root cause of the NRE).
2. Make the BusinessMapper contract explicit: null-check in ToBusinessModel(CreateBusinessRequest), ToBusinessModel(UpdateBusinessRequest) and ToBusinessModel(Business) and throw ArgumentNullException with a clear message instead of a NullReferenceException, so a future caller that skips the guard fails loudly rather than surfacing gRPC Unknown.
3. Add unit tests in BusinessService.Tests/Services/BusinessGrpcTests.cs asserting StatusCode.InvalidArgument for CreateBusinessRequest and UpdateBusinessRequest with no business payload, using the existing FakeBusinessAppService/TestServerCallContext harness; assert the app service was never invoked.
4. Run dotnet build and dotnet test for the BusinessService test project.
Scope note: does NOT change update_mask handling (TASK-7) and does NOT adopt/remove the unused Calzolari validation packages (TASK-11); existing Utils.IsValidBusinessInfo model validation is left in place.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented on branch task-6-null-business-guard (uncommitted, pending user review).

Root cause was ordering, not a missing null check: BusinessGrpc mapped the wire request into a BusinessModel BEFORE running Utils.IsValidBusinessInfo, so BusinessMapper dereferenced request.Business on unvalidated input. proto3 leaves singular message fields unset, so an omitted payload produced a NullReferenceException that escaped as gRPC StatusCode.Unknown - wrong retry semantics (Unknown invites retries, InvalidArgument forbids them) and a client error that looks like a server fault in logs.

Changes:
- BusinessGrpc.CreateBusiness / UpdateBusiness: guard request.Business == null and throw RpcException(InvalidArgument, 'A business payload is required.') before mapping, so validation now precedes mapping.
- BusinessMapper: converted the three ToBusinessModel overloads from expression bodies to blocks with ArgumentNullException.ThrowIfNull on the request and the payload. A direct caller that skips the guard now fails with a named argument instead of an NRE.
- FakeBusinessAppService: added CreateCallCount / UpdateCallCount so tests can assert the rejected request never reaches the application service.
- BusinessGrpcTests: 4 new tests - InvalidArgument for Create and Update with an empty request (plus call-count 0), and ArgumentNullException from both mapper overloads.

Verification: dotnet test BusinessService.Tests -> 18 passed, 0 failed (14 pre-existing + 4 new). Build produced no new warnings in the edited files; the two remaining CS8604/CS8625 warnings are pre-existing in BusinessRepository.cs:209 and UtilsTests.cs:76.

Scope held: update_mask still ignored (TASK-7), unused Calzolari/FluentValidation packages untouched (TASK-11, created from this review). Utils.IsValidBusinessInfo left in place.

Deduplication follow-up after user review: ToBusinessModel(UpdateBusinessRequest) was a verbatim copy of ToBusinessModel(Business) directly above it, re-listing all eight fields. It now delegates via return ToBusinessModel(request.Business), which also promotes ToBusinessModel(Business) from dead code (verified against HEAD: its only apparent call sites, BusinessGrpc.cs:77 and :105, both resolved to the request overloads) to the single definition of the Business -> BusinessModel field mapping. ToBusinessModel(CreateBusinessRequest) cannot delegate because CreateBusinessRequest.business is the distinct newBusiness proto type with no id field and the two share no interface. This cut BusinessMapper.cs from 73 to 62 changed lines.

Final verification: dotnet build MySolution.sln -> Build succeeded, 0 Warning(s), 0 Error(s). dotnet test MySolution.sln -> BusinessService.Tests 18/18 passed, RestAPI.Tests 39/39 passed, 0 failed.

AC evidence (named passing tests): AC#1 CreateBusiness_Throws_InvalidArgument_WhenBusinessPayloadMissing and UpdateBusiness_Throws_InvalidArgument_WhenBusinessPayloadMissing, both also asserting fake.CreateCallCount/UpdateCallCount == 0 to prove the handler short-circuits before reaching IBusinessService. AC#2 BusinessMapper_Throws_ArgumentNullException_WhenCreateBusinessPayloadMissing and BusinessMapper_Throws_ArgumentNullException_WhenUpdateBusinessPayloadMissing, confirming ArgumentNullException replaces NullReferenceException. AC#3 satisfied by those four new tests.
<!-- SECTION:NOTES:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Fixed a NullReferenceException in BusinessService's gRPC create/update paths that surfaced to clients as StatusCode.Unknown instead of InvalidArgument.

Root cause was ordering rather than a missing check: BusinessGrpc mapped the wire request into a BusinessModel before running Utils.IsValidBusinessInfo, so BusinessMapper dereferenced request.Business on unvalidated input. proto3 gives singular message fields no presence guarantee, so an omitted payload threw an NRE that escaped as Unknown - which matters because Unknown invites client retries while InvalidArgument forbids them, and a caller error appeared in logs as a server fault.

Changes: (1) BusinessGrpc.CreateBusiness/UpdateBusiness now guard request.Business == null and throw RpcException(InvalidArgument) before mapping, restoring validate-then-map ordering; (2) BusinessMapper's three ToBusinessModel overloads use ArgumentNullException.ThrowIfNull so a caller bypassing the guard fails with a named argument instead of an opaque NRE; (3) ToBusinessModel(UpdateBusinessRequest) now delegates to ToBusinessModel(Business) instead of duplicating all eight field assignments; (4) FakeBusinessAppService gained CreateCallCount/UpdateCallCount so tests can prove short-circuiting.

Verified with dotnet build MySolution.sln (succeeded, 0 warnings, 0 errors) and dotnet test MySolution.sln (BusinessService.Tests 18/18, RestAPI.Tests 39/39, 0 failed), including four new tests covering the missing-payload case at both the RPC and mapper layers.

Deliberately out of scope: update_mask is still ignored (TASK-7, which touches these same two methods) and the unused Calzolari/FluentValidation package references are untouched (TASK-11, filed from this review).
<!-- SECTION:FINAL_SUMMARY:END -->
