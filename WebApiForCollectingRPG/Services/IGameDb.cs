using System;
using System.Threading.Tasks;

namespace WebApiForCollectingRPG.Services;

public interface IGameDb : IDisposable
{
    public Task<ErrorCode> CreateAccountGameDataAsync(Int64 accountId);
    public Task<ErrorCode> CreateAccountItemDataAsync(Int64 accountId);
}
