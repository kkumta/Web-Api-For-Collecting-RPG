using WebApiForCollectingRPG.ModelDB;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Services;

public class RedisDb : IMemoryDb
{
    public RedisConnection _redisConn;
    private static readonly ILogger<RedisDb> s_logger = GetLogger<RedisDb>();

    public void Init(string address)
    {
        var config = new RedisConfig("default", address);
        _redisConn = new RedisConnection(config);

        s_logger.ZLogDebug($"userDbAddress:{address}");
    }

    public async Task<ErrorCode> RegistUserAsync(String email, String authToken, Int64 accountId)
    {
        var key = MemoryDbKeyMaker.MakeUIDKey(email);
        var result = ErrorCode.None;

        var user = new AuthUser
        {
            Email = email,
            AuthToken = authToken,
            AccountId = accountId,
            State = UserState.Default.ToString()
        };

        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, LoginTimeSpan());
            if (await redis.SetAsync(user, LoginTimeSpan()) == false)
            {
                s_logger.ZLogError(EventIdDic[EventType.LoginAddRedis],
    $"Email:{email}, AuthToken:{authToken},ErrorMessage:UserBasicAuth, RedisString set Error");
                result = ErrorCode.LoginFailAddRedis;
                return result;
            }
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.LoginAddRedis],
                $"Email:{email},AuthToken:{authToken},ErrorMessage:Redis Connection Error");
            result = ErrorCode.LoginFailAddRedis;
            return result;
        }
        return result;
    }

    public async Task<ErrorCode> CheckUserAuthAsync(String email, String authToken)
    {
        var key = MemoryDbKeyMaker.MakeUIDKey(email);
        var result = ErrorCode.None;

        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, null);
            var user = await redis.GetAsync();

            if (!user.HasValue)
            {
                s_logger.ZLogError(EventIdDic[EventType.Login],
    $"RedisDb.CheckUserAuthAsync: Email = {email}, AuthToken = {authToken}, ErrorMessage:ID does Not Exist");
                result = ErrorCode.CheckAuthFailNotExist;
                return result;
            }

            if (user.Value.Email != email || user.Value.AuthToken != authToken)
            {
                s_logger.ZLogError(EventIdDic[EventType.Login],
                    $"RedisDb.CheckUserAuthAsync: Email = {email}, AuthToken = {authToken}, ErrorMessage = Wrong ID or Auth Token");
                result = ErrorCode.CheckAuthFailNotMatch;
                return result;
            }
        }
        catch
        {
            s_logger.ZLogError(EventIdDic[EventType.Login],
    $"RedisDb.CheckUserAuthAsync: Email = {email}, AuthToken = {authToken}, ErrorMessage:Redis Connection Error");
            result = ErrorCode.CheckAuthFailException;
            return result;
        }

        return result;
    }

    public Task<(bool, AuthUser)> GetUserAsync(String email)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> SetUserStateAsync(AuthUser user, UserState userState)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> SetUserReqLockAsync(String key)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> DelUserReqLockAsync(String key)
    {
        throw new System.NotImplementedException();
    }

    public TimeSpan LoginTimeSpan()
    {
        return TimeSpan.FromMinutes(RediskeyExpireTime.LoginKeyExpireMin);
    }

    public TimeSpan TicketKeyTimeSpan()
    {
        return TimeSpan.FromSeconds(RediskeyExpireTime.TicketKeyExpireSecond);
    }

    public TimeSpan NxKeyTimeSpan()
    {
        return TimeSpan.FromSeconds(RediskeyExpireTime.NxKeyExpireSecond);
    }
}