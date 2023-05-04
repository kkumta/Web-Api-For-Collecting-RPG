using System;
using System.Threading.Tasks;

namespace WebApiForCollectingRPG.Services;

public interface IAccountDb : IDisposable
{
    public Task<Tuple<ErrorCode, Int64>> CreateAccountAsync(String email, String password);

    public Task<Tuple<ErrorCode, Int64>> VerifyAccount(String email, String password);

    public Task<Tuple<ErrorCode, Int64>> FindAccountIdByEmail(String email);
}