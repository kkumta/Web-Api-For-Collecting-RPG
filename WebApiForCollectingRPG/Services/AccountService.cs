using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Repository;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Services;

public class AccountService : IAccountService
{
    readonly ILogger<AccountService> _logger;
    readonly IAccountRepository _accountRepository;

    public AccountService(ILogger<AccountService> logger, IAccountRepository accountRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }

    public async Task<Tuple<ErrorCode, Int64>> CreateAccountAsync(String email, String password)
    {
        try
        {
            // password를 salt로 암호화
            var saltValue = Security.SaltString();
            var hashingPassword = Security.MakeHashingPassword(saltValue, password);
            _logger.ZLogDebug(EventIdDic[EventType.AccountService],
                $"[CreateAccount] Email: {email}, SaltValue : {saltValue}, HashingPassword:{hashingPassword}");

            var accountId = _accountRepository.InsertAccountaAsync(email, saltValue, hashingPassword);

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, await accountId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountService], ex,
                $"[AccountService.CreateAccount] ErrorCode: {ErrorCode.CreateAccountFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.CreateAccountFailException, 0);
        }
    }

    public async Task<Tuple<ErrorCode, Int64>> VerifyAccount(String email, String password)
    {
        try
        {

            var accountInfo = await _accountRepository.FindAccountByEmailAsync(email);

            _logger.ZLogDebug(EventIdDic[EventType.AccountService],
                    $"[VerifyAccount] AccountId: {accountInfo.AccountId} Email: {accountInfo.Email}");

            if (accountInfo is null || accountInfo.AccountId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.AccountService],
                    $"[AccountService.VerifyAccount] ErrorCode: {ErrorCode.LoginFailUserNotExist}, AccountId: {accountInfo.AccountId} Email: {accountInfo.Email} SaltValue: {accountInfo.SaltValue}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailUserNotExist, 0);
            }

            var hashingPassword = Security.MakeHashingPassword(accountInfo.SaltValue, password);
            if (accountInfo.HashedPassword != hashingPassword)
            {
                _logger.ZLogError(EventIdDic[EventType.AccountService],
                    $"[AccountService.VerifyAccount] ErrorCode: {ErrorCode.LoginFailPasswordNotMatch}, Email: {email}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailPasswordNotMatch, 0);
            }

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountInfo.AccountId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountService], ex,
                $"[AccountService.VerifyAccount] ErrorCode: {ErrorCode.LoginFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailException, 0);
        }
    }

    public async Task<Tuple<ErrorCode, Int64>> FindAccountIdByEmail(String email)
    {
        try
        {
            var accountId = await _accountRepository.FindAccountIdByEmailAsync(email);

            if (accountId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.AccountService],
                    $"[AccountService.FindAccountIdByEmail] ErrorCode: {ErrorCode.FindAccountIdByEmailFailException}, Email: {email}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.FindAccountIdByEmailFailNotExist, 0);
            }

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.AccountService], ex,
                $"[AccountService.FindAccountIdByEmail] ErrorCode: {ErrorCode.FindAccountIdByEmailFailException}, Email: {email}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.FindAccountIdByEmailFailException, 0);
        }
    }
}