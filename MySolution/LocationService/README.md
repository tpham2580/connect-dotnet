# .NET 8 gRPC Location Service

This project is built using **.NET 8** and follows a modern microservice architecture pattern. It includes:
The Location Service uses the Redis Geospatial index to store business coordinates.

```bash
grpcurl -plaintext \
  -d '{"latitude":47.61,"longitude":-122.33,"radiusM":5000,"limit":5}' \
  localhost:6000 \
  location.v1.LocationService/GetNearbyBusinesses
```

## Functional

- [] Search for locations within provided radius
- [] Add location

### TODO

- [x] Connect service to Redis Geospatial index
- [] Add calls to interact with Redis
