---
id: TASK-5
title: Fix non-functional ItemsController in RestAPI
status: To Do
assignee: []
created_date: '2026-07-23 04:10'
labels:
  - restapi
  - bug
dependencies: []
priority: medium
ordinal: 5000
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
RestAPI ItemsController has a private constructor and a private GetById action, so the endpoint is not routable and DI cannot activate it. IItemService/ItemService are never registered in Program.cs and the in-memory store is always empty. Make the members public and register the service, or remove the dead feature.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Controller constructor and GetById action are public and reachable via routing
- [ ] #2 IItemService is registered in DI in Program.cs
- [ ] #3 GET /items/{id} returns 200 for a known id and 404 otherwise (or the feature is removed if not needed)
<!-- AC:END -->
