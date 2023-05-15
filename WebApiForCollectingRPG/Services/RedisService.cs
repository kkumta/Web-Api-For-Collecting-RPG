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
using System.Collections.Generic;
using System.Linq;

namespace WebApiForCollectingRPG.Services;

public class RedisService : IMemoryService
{
    public RedisConnection _redisConn;
    private static readonly ILogger<RedisService> s_logger = GetLogger<RedisService>();
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IMemoryCacheService _memoryCacheService;

    public RedisService(IHttpContextAccessor httpContextAccessor, IMemoryCacheService memoryCacheService)
    {
        _httpContextAccessor = httpContextAccessor;
        _memoryCacheService = memoryCacheService;
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
                    $"RedisService.GetNoticeAsync: key = {key}, value = {notice.Value}");

            return (ErrorCode.None, notice.Value);
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.ReceiveNotice], ex,
                $"RedisService.GetNoticeAsync: key = {key}, ErrorMessage = Applicable Notice Not Exist, ErrorCode: {ErrorCode.GetNoticeException}");
            return (ErrorCode.GetNoticeException, null);
        }
    }

    public async Task<ErrorCode> ItemFarmingAsync(Int32 stageId, Int64 itemId)
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
            s_logger.ZLogDebug(EventIdDic[EventType.ItemFarming],
                $"RedisService.ItemFarmingAsync: Key = {key}, Message = Success Farm Item Number {itemId}!");

            return ErrorCode.None;
        }
        catch (Exception ex) {
            s_logger.ZLogError(EventIdDic[EventType.ItemFarming], ex,
                $"RedisService.ItemFarmingAsync: stageId = {stageId}, itemId = {itemId}, ErrorMessage = ItemFarming Exception, ErrorCode: {ErrorCode.ItemFarmingException}");
            return ErrorCode.ItemFarmingException;
        }
    }

    public async Task<(ErrorCode, bool, List<Int64>)> KillNpcAsync(Int32 stageId, Int32 npcId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return (errorCode, false, null);
            }

            var key = MemoryDbKeyMaker.MakePlayerStageKillingNpcKey(playerId, stageId);
            var redis = new RedisList<Int32>(_redisConn, key, StageKeyTimeSpan());
            await redis.RightPushAsync(npcId);

            s_logger.ZLogDebug(EventIdDic[EventType.KillNpc],
                $"RedisService.KillNpcAsync: Key = {key}, Message = Success Kill Npc number {npcId}!");

            Int32 totalNpcCount = 0;
            (errorCode, var npcs) = _memoryCacheService.GetAttackNpcsByStageId(stageId);
            if (errorCode != ErrorCode.None)
            {
                return (errorCode, false, null);
            }
            foreach ( var npc in npcs )
            {
                totalNpcCount += npc.NpcCount;
            }

            if (await redis.LengthAsync() >= totalNpcCount)
            {
                (errorCode, var items) = await GetFarmedItemsAndDeleteAsync(playerId, stageId);
                if (errorCode != ErrorCode.None)
                {
                    return (errorCode, false, null);
                }

                await redis.DeleteAsync();

                s_logger.ZLogDebug(EventIdDic[EventType.KillNpc],
                    $"RedisService.KillNpcAsync: Key = {key}, Message = Success Clear Stage Number {stageId}!");

                return (ErrorCode.None, true, items);
            }

            return (ErrorCode.None, false, null);
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.KillNpc], ex,
                $"RedisDb.ItemFarming: stageId = {stageId}, npcId = {npcId}, ErrorMessage = KillNpc Exception, ErrorCode: {ErrorCode.KillNpcException}");
            return (ErrorCode.KillNpcException, false, null);
        }
    }

    private async Task<(ErrorCode, List<Int64>)> GetFarmedItemsAndDeleteAsync(Int64? playerId, Int32 stageId)
    {
        try {
            var key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(playerId, stageId);
            var redis = new RedisList<Int64>(_redisConn, key, StageKeyTimeSpan());

            var result = await redis.RangeAsync(0, -1);
            await redis.DeleteAsync();

            return (ErrorCode.None, result.ToList());
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.RedisService], ex,
                $"MemoryService.GetFarmedItems: playerId = {playerId}, stageId = {stageId}, ErrorMessage = KillNpc Exception, ErrorCode: {ErrorCode.GetFarmedItemsException}");
            return (ErrorCode.GetFarmedItemsException, null);
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