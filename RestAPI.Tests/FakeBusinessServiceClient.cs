using System.Collections.Concurrent;
using Grpc.Core;
using GrpcBusiness = Grpc.BusinessService;

namespace RestAPI.Tests;

/// <summary>
/// In-memory stand-in for the BusinessService gRPC client so integration tests
/// exercise the full REST + mapping stack without a running BusinessService.
/// </summary>
public class FakeBusinessServiceClient : GrpcBusiness.BusinessService.BusinessServiceClient
{
    private readonly ConcurrentDictionary<long, GrpcBusiness.Business> _store = new();
    private long _nextId;

    public CancellationToken LastCancellationToken { get; private set; }

    public FakeBusinessServiceClient(IEnumerable<GrpcBusiness.Business>? seed = null)
    {
        if (seed == null)
        {
            return;
        }

        foreach (var business in seed)
        {
            _store[business.Id] = business;
            if (business.Id > _nextId)
            {
                _nextId = business.Id;
            }
        }
    }

    public override AsyncUnaryCall<GrpcBusiness.ListBusinessesResponse> ListBusinessesAsync(
        GrpcBusiness.ListBusinessesRequest request, CallOptions options)
    {
        LastCancellationToken = options.CancellationToken;

        var ordered = _store.Values.OrderBy(b => b.Id).ToList();
        var limit = request.Limit <= 0 ? ordered.Count : request.Limit;
        var page = ordered.Skip(request.Offset).Take(limit);

        var response = new GrpcBusiness.ListBusinessesResponse { Total = ordered.Count };
        response.Businesses.AddRange(page);
        return Success(response);
    }

    public override AsyncUnaryCall<GrpcBusiness.BusinessResponse> GetBusinessByIdAsync(
        GrpcBusiness.BusinessByIdRequest request, CallOptions options)
    {
        if (_store.TryGetValue(request.Id, out var business))
        {
            return Success(new GrpcBusiness.BusinessResponse { Business = business });
        }

        return Failure<GrpcBusiness.BusinessResponse>(
            new RpcException(new Status(StatusCode.NotFound, $"Business with ID {request.Id} not found.")));
    }

    public override AsyncUnaryCall<GrpcBusiness.BusinessResponse> CreateBusinessAsync(
        GrpcBusiness.CreateBusinessRequest request, CallOptions options)
    {
        var id = Interlocked.Increment(ref _nextId);
        var created = new GrpcBusiness.Business
        {
            Id = id,
            Name = request.Business.Name,
            Address = request.Business.Address,
            City = request.Business.City,
            State = request.Business.State,
            Country = request.Business.Country,
            Latitude = request.Business.Latitude,
            Longitude = request.Business.Longitude
        };

        _store[id] = created;
        return Success(new GrpcBusiness.BusinessResponse { Business = created });
    }

    public override AsyncUnaryCall<GrpcBusiness.BusinessResponse> UpdateBusinessAsync(
        GrpcBusiness.UpdateBusinessRequest request, CallOptions options)
    {
        var id = request.Business.Id;
        if (!_store.ContainsKey(id))
        {
            return Failure<GrpcBusiness.BusinessResponse>(
                new RpcException(new Status(StatusCode.NotFound, $"Business with ID {id} not found.")));
        }

        var updated = new GrpcBusiness.Business
        {
            Id = id,
            Name = request.Business.Name,
            Address = request.Business.Address,
            City = request.Business.City,
            State = request.Business.State,
            Country = request.Business.Country,
            Latitude = request.Business.Latitude,
            Longitude = request.Business.Longitude
        };

        _store[id] = updated;
        return Success(new GrpcBusiness.BusinessResponse { Business = updated });
    }

    public override AsyncUnaryCall<GrpcBusiness.DeleteItemByIdResponse> DeleteBusinessAsync(
        GrpcBusiness.BusinessByIdRequest request, CallOptions options)
    {
        if (_store.TryRemove(request.Id, out _))
        {
            return Success(new GrpcBusiness.DeleteItemByIdResponse { Success = true });
        }

        return Failure<GrpcBusiness.DeleteItemByIdResponse>(
            new RpcException(new Status(StatusCode.NotFound, $"Business with ID {request.Id} not found.")));
    }

    private static AsyncUnaryCall<T> Success<T>(T response) =>
        new AsyncUnaryCall<T>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    private static AsyncUnaryCall<T> Failure<T>(RpcException exception) =>
        new AsyncUnaryCall<T>(
            Task.FromException<T>(exception),
            Task.FromResult(new Metadata()),
            () => exception.Status,
            () => exception.Trailers,
            () => { });
}
