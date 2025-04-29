# .NET 8 gRPC Business Service

This project is built using **.NET 8** and follows a modern microservice architecture pattern. It includes:
The Business Service uses PostgreSQL to manage business information.

```bash
grpcurl -plaintext -d '{"id": 1}' localhost:6001 business.v1.BusinessService/GetBusinessById
```

## TODO

- [x] Connect to PostgreSQL server
- [x] Get Business Info by Id
- [ ] Get Businesses from list of Ids
- [ ] Create Business
- [ ] Update Business Info
- [ ] Delete Business
