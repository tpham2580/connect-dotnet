# .NET 8 gRPC Business Service

This project is built using **.NET 8** and follows a modern microservice architecture pattern.  
The Business Service uses PostgreSQL to manage business information.

## Example gRPC Calls

### Get a business by ID

```bash
grpcurl -plaintext -d '{"id": 1}' localhost:6001 business.v1.BusinessService/GetBusinessById
```

### Create a new business

```bash
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

### Update an existing business

```bash
grpcurl -plaintext \
  -d '{
        "business": {
          "id": 3,
          "name":      "Updated Coffee",
          "address":   "456 Market St",
          "city":      "Seattle",
          "state":     "WA",
          "country":   "USA",
          "latitude":  47.6100,
          "longitude": -122.3340
        },
        "update_mask": {
          "paths": [
            "name",
            "address",
            "city",
            "state",
            "country",
            "latitude",
            "longitude"
          ]
        }
      }' \
  localhost:6001 \
  business.v1.BusinessService/UpdateBusiness
```

## TODO

- [x] Connect to PostgreSQL server  
- [x] Get Business Info by Id  
- [ ] Get Businesses from list of Ids  
- [x] Create Business  
- [x] Update Business Info  
- [ ] Delete Business  
