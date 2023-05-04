using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Dtos.Game;

namespace WebApiForCollectingRPG.Services;

public interface IGameDb : IDisposable
{
    public Task<ErrorCode> CreateAccountGameDataAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, AccountGame>> GetAccountGameInfoAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, IEnumerable<AccountItem>>> GetAccountItemListAsync(Int64 accountId);
}