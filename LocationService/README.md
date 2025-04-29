# .NET 8 gRPC Location Service

This project is built using **.NET 8** and follows a modern microservice architecture pattern. It includes:
The Location Service uses the Redis Geospatial index to store business coordinates.

```bash
grpcurl -plaintext \
  -d '{"latitude":47.582400761924504,"longitude":-122.16794708655202,"radiusM":5000000,"limit":100}' \
  localhost:6000 \
  location.v1.LocationService/GetNearbyBusinesses
```

## TODO

- [x] Connect service to Redis Geospatial index
- [ ] Add calls to interact with Redis (GEOADD, GEOSEARCH)
- [x] Add call to get business names by list of ids from redis ()
