    using Grpc.Core;
using Iam.Grpc;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Presentation.Grpc;

// gRPC UserService for IAM. Queries real data from LabIAM database when available.
public class IamGrpcUserService : Iam.Grpc.UserService.UserServiceBase
{
    private readonly IamDbContext _db;

    public IamGrpcUserService(IamDbContext db)
    {
        _db = db;
    }

    public override async Task<GetUserReply> GetUser(GetUserRequest request, ServerCallContext context)
    {
        // Try to parse incoming userId
        var ok = Guid.TryParse(request.UserId, out var id);

        if (!ok || id == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid userId"));
        }

        // Query database; if DB unreachable in container, this will throw. You can provide SQL auth via ConnectionStrings__LabIAM.
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id, context.CancellationToken);

        if (user == null)
        {
            // Not found -> return minimal info or throw NotFound
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        return new GetUserReply
        {
            UserId = user.UserId.ToString(),
            FullName = user.FullName ?? user.Username,
            Email = user.Email
        };
    }
}
