---
id: TASK-4
title: Pin .NET SDK version in docker-compose
status: Done
assignee: []
created_date: '2026-07-23 01:58'
labels:
  - devops
dependencies: []
references:
  - 'https://github.com/tpham2580/connect-dotnet/pull/37'
ordinal: 4000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Pin the SDK image tag so local and CI builds are reproducible.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 docker-compose uses an explicit SDK tag
<!-- AC:END -->

## Final Summary

<!-- SECTION:FINAL_SUMMARY:BEGIN -->
Pinned SDK to 8.0.x across compose files; verified clean build.
<!-- SECTION:FINAL_SUMMARY:END -->
