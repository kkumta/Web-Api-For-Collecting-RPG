using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Data;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Controllers;
using WebApiForCollectingRPG.DAO;
using ZLogger;
using static LogManager;

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

    public async Task<Tuple<ErrorCode, Int64>> CreateAccountAsync(String email, String password)
    {
        try
        {
            // password를 salt로 암호화
            var saltValue = Security.SaltString();
            var hashingPassword = Security.MakeHashingPassword(saltValue, password);
            _logger.ZLogDebug(EventIdDic[EventType.AccountDb],
                $"[CreateAccount] Email: {email}, SaltValue : {saltValue}, HashingPassword:{hashingPassword}");

            var accountId = await _queryFactory.Query("Account").InsertGetIdAsync<Int64>(new
            {
                email = email,
                salt_value = saltValue,
                hashed_password = hashingPassword
            });

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountDb], ex,
                $"[AccountDb.CreateAccount] ErrorCode: {ErrorCode.CreateAccountFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.CreateAccountFailException, 0);
        }
    }

    public async Task<Tuple<ErrorCode, Int64>> VerifyAccount(String email, String password)
    {
        try
        {

            var accountInfo = await _queryFactory.Query("account")
                .Where("email", email)
                .Select("account_id AS AccountId",              
                "email AS Email",
                "hashed_password AS HashedPassword",
                "salt_value AS SaltValue")
                .FirstOrDefaultAsync();

            _logger.ZLogDebug(EventIdDic[EventType.AccountDb],
    $"[VerifyAccount] AccountId: {accountInfo.AccountId} Email: {accountInfo.Email}");

            if (accountInfo is null || accountInfo.AccountId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.AccountDb],
$"[AccountDb.VerifyAccount] ErrorCode: {ErrorCode.LoginFailUserNotExist}, AccountId: {accountInfo.AccountId} Email: {accountInfo.Email} SaltValue: {accountInfo.SaltValue}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailUserNotExist, 0);
            }

            var hashingPassword = Security.MakeHashingPassword(accountInfo.SaltValue, password);
            if (accountInfo.HashedPassword != hashingPassword)
            {
                _logger.ZLogError(EventIdDic[EventType.AccountDb],
    $"[AccountDb.VerifyAccount] ErrorCode: {ErrorCode.LoginFailPasswordNotMatch}, Email: {email}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailPasswordNotMatch, 0);
            }

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountInfo.AccountId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountDb], ex,
    $"[AccountDb.VerifyAccount] ErrorCode: {ErrorCode.LoginFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailException, 0);
        }
    }

    public async Task<Tuple<ErrorCode, Int64>> FindAccountIdByEmail(String email)
    {
        try
        {
            var accountId = await _queryFactory.Query("account")
                    .Where("email", email)
                    .Select("account_id AS AccountId")
                    .FirstOrDefaultAsync<Int64>();

            if (accountId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.AccountDb],
$"[AccountDb.FindAccountIdByEmail] ErrorCode: {ErrorCode.FindAccountIdByEmailFailException}, Email: {email}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.FindAccountIdByEmailFailNotExist, 0);
            }

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountDb], ex,
$"[AccountDb.FindAccountIdByEmail] ErrorCode: {ErrorCode.FindAccountIdByEmailFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.FindAccountIdByEmailFailException, 0);
        }
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
            _logger.ZLogError(EventIdDic[EventType.AccountDb], ex,
$"[AccountDb.Open] ErrorCode: {ErrorCode.GetAccountDbConnectionFail}");
        }
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