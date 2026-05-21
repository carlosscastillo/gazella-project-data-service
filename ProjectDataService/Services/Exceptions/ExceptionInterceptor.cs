using Grpc.Core;
using Grpc.Core.Interceptors;
using ProjectDataService.Data.Exceptions;

namespace ProjectDataService.Services.Exceptions;

public class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    private const string GazellaError     = "x-gazella-error";
    private const string InvalidArgument  = "invalid_argument";
    private const string InvalidOperation = "invalid_operation";
    private const string DbUnavailable    = "db_unavailable";
    private const string NotFound         = "not_found";
    private const string Aborted          = "aborted";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (GazellaValidationException ex)
        {
            var metadata = new Metadata { { GazellaError, InvalidArgument } };
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Issues), metadata);
        }
        catch (GazellaInvalidOperationException ex)
        {
            var metadata = new Metadata { { GazellaError, InvalidOperation } };
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message), metadata);
        }
        catch (GazellaNotFoundException ex)
        {
            var metadata = new Metadata { { GazellaError, NotFound } };
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message), metadata);
        }
        catch (GazellaConcurrencyException ex)
        {
            var metadata = new Metadata { { GazellaError, Aborted } };
            throw new RpcException(new Status(StatusCode.Aborted, ex.Message), metadata);
        }
        catch (GazellaDbException)
        {
            var metadata = new Metadata { { GazellaError, DbUnavailable } };
            throw new RpcException(
                new Status(StatusCode.Unavailable,
                    "The database is not available or took too long to respond"),
                metadata);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception while processing {Method}: {Ex}", context.Method, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }
}