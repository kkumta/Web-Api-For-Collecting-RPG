using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Data;
using System.Threading.Tasks;
using WebApiForCollectingRPG.ModelDB;
using ZLogger;

namespace WebApiForCollectingRPG.Services;

public class AccountDb : IAccountDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<AccountDb> _logger;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public AccountDb(ILogger<AccountDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConn, _compiler);
    }

    public async Task<ErrorCode> CreateAccountAsync(String email, String password)
    {
        try
        {
            // password를 salt로 암호화
            var saltValue = Security.SaltString();
            var hashingPassword = Security.MakeHashingPassword(saltValue, password);
            _logger.ZLogDebug(
                $"[CreateAccount] Email: {email}, SaltValue : {saltValue}, HashingPassword:{hashingPassword}");

            var count = await _queryFactory.Query("account").InsertAsync(new
            {
                Email = email,
                SaltValue = saltValue,
                HashedPassword = hashingPassword
            });

            if (count != 1)
            {
                return ErrorCode.CreateAccountFailInsert;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
            _logger.ZLogError(e,
                $"[AccountDb.CreateAccount] ErrorCode: {ErrorCode.CreateAccountFailException}, Email: {email}");
            return ErrorCode.CreateAccountFailException;
        }
    }

    public async Task<Tuple<ErrorCode, Int64>> VerifyAccount(string email, string password)
    {
        try
        {
            var accountInfo = await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();

            if (accountInfo is null || accountInfo.AccountId == 0)
            {
                return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailUserNotExist, 0);
            }

            var hashingPassword = Security.MakeHashingPassword(accountInfo.SaltValue, password);
            if (accountInfo.HashedPassword != hashingPassword)
            {
                _logger.ZLogError(
    $"[AccountDb.VerifyAccount] ErrorCode: {ErrorCode.LoginFailPasswordNotMatch}, Email: {email}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailPasswordNotMatch, 0);
            }

            return new Tuple<ErrorCode, long>(ErrorCode.None, accountInfo.AccountId);

        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex,
    $"[AccountDb.VerifyAccount] ErrorCode: {ErrorCode.LoginFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailException, 0);
        }
    }

    public void Dispose()
    {
        Close();
    }

    private void Open()
    {
        _dbConn = new MySqlConnection(_dbConfig.Value.AccountDb);
        _logger.ZLogError(
$"[AccountDb.Open] ErrorCode: {ErrorCode.GetAccountDbConnectionFail}, _dbConfig: {_dbConfig.ToString}");
        _dbConn.Open();
    }

    private void Close()
    {
        _dbConn.Close();
    }
}

public class DbConfig
{
    public String MasterDb { get; set; }
    public String AccountDb { get; set; }
    public String GameDb { get; set; }
    public String Memcached { get; set; }
}