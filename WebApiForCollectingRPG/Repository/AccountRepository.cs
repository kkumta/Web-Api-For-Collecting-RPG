using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Data;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;
using static LogManager;
using ZLogger;

namespace WebApiForCollectingRPG.Repository;

public class AccountRepository : IAccountRepository
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<AccountRepository> _logger;

    IDbConnection _dbConn;
    MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public AccountRepository(IOptions<DbConfig> dbConfig, ILogger<AccountRepository> logger)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        Open();

        _compiler = new MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConn, _compiler);
    }

    public void Dispose()
    {
        Close();
    }

    private void Open()
    {
        try
        {
            _dbConn = new MySqlConnection(_dbConfig.Value.AccountDb);
            _dbConn.Open();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountRepository], ex,
                $"[AccountRepository.Open] ErrorCode: {ErrorCode.GetAccountDbConnectionFail}");
        }
    }

    private void Close()
    {
        _dbConn.Close();
    }

    public async Task<Int64> InsertAccountaAsync(String email, String saltValue, String hashingPassword)
    {
        return await _queryFactory.Query("Account").InsertGetIdAsync<Int64>(new
        {
            email = email,
            salt_value = saltValue,
            hashed_password = hashingPassword
        });
    }

    public async Task<Account> FindAccountByEmailAsync(String email)
    {
        return await _queryFactory.Query("account")
                .Where("email", email)
                .Select("account_id AS AccountId",
                "email AS Email",
                "hashed_password AS HashedPassword",
                "salt_value AS SaltValue")
                .FirstOrDefaultAsync<Account>();
    }

    public async Task<Int64> FindAccountIdByEmailAsync(String email)
    {
        return await _queryFactory.Query("account")
                    .Where("email", email)
                    .Select("account_id AS AccountId")
                    .FirstOrDefaultAsync<Int64>();
    }

    public async void DeleteAccountAsync(Int64 accountId)
    {
        await _queryFactory.Query("account").Where("account_id", accountId).DeleteAsync();
    }
}

public class DbConfig
{
    public String MasterDb { get; set; }
    public String AccountDb { get; set; }
    public String GameDb { get; set; }
    public String Memcached { get; set; }
}