using WebApiForCollectingRPG.DAO;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.Util;
using Microsoft.AspNetCore.Http;

namespace WebApiForCollectingRPG.Services;

public class RedisService : IMemoryService
{
    public RedisConnection _redisConn;
    private static readonly ILogger<RedisService> s_logger = GetLogger<RedisService>();
    readonly IHttpContextAccessor _httpContextAccessor;

    public RedisService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Init(String address)
    {
        var config = new RedisConfig("default", address);
        _redisConn = new RedisConnection(config);

        s_logger.ZLogDebug($"userDbAddress:{address}");
    }

    public async Task<ErrorCode> RegistUserAsync(String email, String authToken, Int64 accountId, Int64 playerId)
    {
        var key = MemoryDbKeyMaker.MakeUIDKey(email);
        var result = ErrorCode.None;

        var user = new AuthUser
        {
            Email = email,
            AuthToken = authToken,
            AccountId = accountId,
            PlayerId = playerId
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
            s_logger.ZLogError(EventIdDic[EventType.LoginAddRedis], ex,
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

    public async Task<(bool, AuthUser)> GetUserAsync(String id)
    {
        var uid = MemoryDbKeyMaker.MakeUIDKey(id);

        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, uid, null);
            var user = await redis.GetAsync();
            if (!user.HasValue)
            {
                s_logger.ZLogError(
                    $"RedisDb.UserStartCheckAsync: UID = {uid}, ErrorMessage = Not Assigned User, RedisString get Error");
                return (false, null);
            }

            return (true, user.Value);
        }
        catch
        {
            s_logger.ZLogError($"UID:{uid},ErrorMessage:ID does Not Exist");
            return (false, null);
        }
    }

    public async Task<bool> SetUserReqLockAsync(String key)
    {
        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, NxKeyTimeSpan());
            if (await redis.SetAsync(new AuthUser
            {
                // emtpy value
            }, NxKeyTimeSpan(), StackExchange.Redis.When.NotExists) == false)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    public async Task<bool> DelUserReqLockAsync(String key)
    {
        if (String.IsNullOrEmpty(key))
        {
            return false;
        }

        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, null);
            var redisResult = await redis.DeleteAsync();
            return redisResult;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(ErrorCode, Notice)> GetNoticeAsync()
    {
        Int32 id = 1;
        var key = MemoryDbKeyMaker.MakeNoticeKey(id);

        try
        {
            var redis = new RedisString<Notice>(_redisConn, key, null);

            var notice = await redis.GetAsync();
            if (!notice.HasValue)
            {
                s_logger.ZLogError(EventIdDic[EventType.ReceiveNotice],
                    $"RedisDb.GetNoticeAsync: key = {key}, ErrorMessage = Applicable Notice Not Exist, ErrorCode: {ErrorCode.GetNoticeFailNotExist}");

                return (ErrorCode.GetNoticeFailNotExist, null);
            }
            s_logger.ZLogDebug(EventIdDic[EventType.ReceiveNotice],
                    $"RedisDb.GetNoticeAsync: key = {key}, value = {notice.Value}");

            return (ErrorCode.None, notice.Value);
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.ReceiveNotice], ex,
                $"RedisDb.GetNoticeAsync: key = {key}, ErrorMessage = Applicable Notice Not Exist, ErrorCode: {ErrorCode.GetNoticeException}");
            return (ErrorCode.GetNoticeException, null);
        }
    }

    public async Task<ErrorCode> ItemFarming(Int32 stageId, Int64 itemId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            var key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(playerId, stageId);
            var redis = new RedisList<Int64>(_redisConn, key, StageKeyTimeSpan());
            await redis.RightPushAsync(itemId);

            return ErrorCode.None;
        }
        catch (Exception ex) {
            s_logger.ZLogError(EventIdDic[EventType.ItemFarming], ex,
                $"RedisDb.ItemFarming: stageId = {stageId}, itemId = {itemId}, ErrorMessage = ItemFarming Exception, ErrorCode: {ErrorCode.ItemFarmingException}");
            return ErrorCode.ItemFarmingException;
        }
    }

    private (ErrorCode, Int64?) GetPlayerIdFromHttpContext()
    {
        var playerId = (_httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser)?.PlayerId;

        if (playerId == null)
        {
            s_logger.ZLogError(EventIdDic[EventType.GameService],
                $"[GameService.GetPlayerIdFromHttpContext] ErrorCode: {ErrorCode.PlayerIdNotExist}");
            return new(ErrorCode.PlayerIdNotExist, playerId);
        }

        return (ErrorCode.None, playerId);
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

    public TimeSpan StageKeyTimeSpan()
    {
        return TimeSpan.FromSeconds(RediskeyExpireTime.StageExpireSecond);
    }
}