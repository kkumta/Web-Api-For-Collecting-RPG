using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;

namespace WebApiForCollectingRPG.Repository;

public interface IAccountRepository : IDisposable
{
    public Task<Int64> InsertAccountaAsync(String email, String saltValue, String hashingPassword);
    public Task<Account> FindAccountByEmailAsync(String email);
    public Task<Int64> FindAccountIdByEmailAsync(String email);
}