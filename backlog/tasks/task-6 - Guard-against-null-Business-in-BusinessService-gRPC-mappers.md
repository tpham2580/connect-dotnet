---
id: TASK-6
title: Guard against null Business in BusinessService gRPC mappers
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
labels:
  - businessservice
  - bug
dependencies: []
priority: medium
ordinal: 6000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
proto3 singular message fields can be unset, but BusinessMapper dereferences request.Business.* with no null check in CreateBusiness/UpdateBusiness. A missing payload surfaces as a gRPC Unknown/NRE instead of InvalidArgument.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 CreateBusiness and UpdateBusiness return InvalidArgument when the business payload is missing
- [ ] #2 BusinessMapper no longer throws NullReferenceException on a null Business
- [ ] #3 Unit test covers the missing-business case
<!-- AC:END -->
