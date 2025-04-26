# .NET 8 REST API

The REST API serves as an API Gateway for a .NET Microservice Architecture

#### API Endpoints

<details>
 <summary><code>GET</code> <code><b>/v1/search/nearby</b></code> <code>(Returns businesses based on location)</code></summary>

##### Request Parameters

> | field     |  type     | data type               | description                                                           |
> |-----------|-----------|-------------------------|-----------------------------------------------------------------------|
> | Longitude |  required | decimal                 | Latitude of a given location  |
> | Latitude  |  required | decimal                 | Longitude of a given location  |
> | Radius    |  optional | int                     | Default is 5000m (about 3 miles)  |


##### Responses

> | http code     | content-type                      | response                                                            |
> |---------------|-----------------------------------|---------------------------------------------------------------------|
> | `200`         | `application/json`        | `{"total": INT, "businesses": [{business object}]}`                                |
> | `400`         | `application/json`                | `{"code":"400","message":"Bad Request"}`                            |


</details>

---

## TODO

- [ ] CRUD Functionality for Business Information
    - [ ] Read Business Info
    - [ ] Create Business Info
    - [ ] Update Business Info
    - [ ] Delete Business info
- [ ] API call search nearby businesses
    - [x] Expose endpoint
    - [ ] Remove dummy data and setup gRPC client
    - [ ] Returns proper data from LocationService
- [ ] Rate limiting
