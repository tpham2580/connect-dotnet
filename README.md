# .NET 8 Microservices Project

This project is built using **.NET 8** and follows a modern microservice architecture pattern. It includes:

- A **REST API Service**
- A **gRPC Microservices**

## Functional

### Proximity Service

- [] Return all businesses based on a user's location.
- [] Business owners can add, delete, or update a business, but info does not need to be reflected in real-time.
- [] Customers can view detailed information about a business.

-----------------------------------------

### Nearby Friends

- [] Users should see nearby friends on mobile app. Each entry in the nearby friend list has a distance and a timestamp indicated when the distance was last updated. 
- [] Nearby friends list should be updated every few seconds.

## Non-Functional

- [] Low latency. 

### Proximity Service

- [] Low latency. Users should be able to see nearby businesses quickly.
- [] Data Privacy. Compliant with data privacy laws like General Data Protection Regulation (GDPR).
- [] High Availability and Scalability requirements. Handle spikes in densely populated areas.

### Nearby Friends

- [] Reliable overall but ocassional data point loss is acceptable.
- [] Eventual Consistency. A few seconds delay in receiving location data in different replicas is acceptable.
