# .NET 8 gRPC Business Service

This project is built using **.NET 8** and follows a modern microservice architecture pattern. It includes:
The Business Service uses PostgreSQL to manage business information.

```bash
grpcurl -plaintext -d '{"id": 1}' localhost:6001 business.v1.BusinessService/GetBusinessById

grpcurl -plaintext \
  -d '{
        "business": {
          "name":      "Example Coffee",
          "address":   "123 Main St",
          "city":      "Seattle",
          "state":     "WA",
          "country":   "USA",
          "latitude":  47.6097,
          "longitude": -122.3331
        }
      }' \
  localhost:6001 \
  business.v1.BusinessService/CreateBusiness
```

## TODO

- [x] Connect to PostgreSQL server
- [x] Get Business Info by Id
- [ ] Get Businesses from list of Ids
- [ ] Create Business
- [ ] Update Business Info
- [ ] Delete Business
