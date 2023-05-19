using WebApiForCollectingRPG.DAO;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.Util;
using System.Collections.Generic;
using System.Linq;
using WebApiForCollectingRPG.DTO.Dungeon;
using StackExchange.Redis;

namespace WebApiForCollectingRPG.Services;

public class RedisService : IMemoryService
{
    public RedisConnection _redisConn;
    private static readonly ILogger<RedisService> s_logger = GetLogger<RedisService>();
    readonly IMemoryCacheService _memoryCacheService;

    public RedisService(IMemoryCacheService memoryCacheService)
    {
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
            PlayerId = playerId,
            State = UserState.Default.ToString()
        };

        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, key, LoginTimeSpan());
            if (!await redis.SetAsync(user, LoginTimeSpan()) || !await SetUserStateAsync(user, UserState.Login))
            {
                s_logger.ZLogError(EventIdDic[EventType.LoginAddRedis],
                    $"Email:{email}, AuthToken:{authToken}, ErrorMessage: User Basic Auth, RedisString set Error");
                result = ErrorCode.LoginFailAddRedis;
                return result;
            }
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.LoginAddRedis], ex,
                $"Email:{email},AuthToken:{authToken},ErrorMessage: Redis Connection Error");
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

    public async Task<bool> SetUserStateAsync(AuthUser user, UserState userState)
    {
        var uid = MemoryDbKeyMaker.MakeUIDKey(user.Email);
        try
        {
            var redis = new RedisString<AuthUser>(_redisConn, uid, null);

            user.State = userState.ToString();

            if (await redis.SetAsync(user) == false)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
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

    public async Task<ErrorCode> ItemFarmingAsync(String email, Int32 stageId, Int64 itemId)
    {
        try
        {
            var errorCode = ErrorCode.ItemNotExist;

            var key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(email, stageId);
            var redis = new RedisList<RedisItemDTO>(_redisConn, key, StageKeyTimeSpan());

            var items = await redis.RangeAsync(0, -1);
            for (var idx = 0; idx < items.Length; idx++)
            {
                var item = items[idx];
                if (item.ItemId == itemId)
                {
                    if (item.CurCount >= item.TotalCount)
                    {
                        return ErrorCode.OverTotalItemCount;
                    }

                    item.CurCount++;
                    items[idx] = item;

                    errorCode = ErrorCode.None;

                    break;
                }
            }

            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            await redis.DeleteAsync();
            await redis.RightPushAsync(items);

            s_logger.ZLogDebug(EventIdDic[EventType.ItemFarming],
                $"RedisService.ItemFarmingAsync: Key = {key}, Message = Success Farm Item Number {itemId}!");

            return errorCode;
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.ItemFarming], ex,
                $"RedisService.ItemFarmingAsync: stageId = {stageId}, itemId = {itemId}, ErrorMessage = ItemFarming Exception, ErrorCode: {ErrorCode.ItemFarmingException}");
            return ErrorCode.ItemFarmingException;
        }
    }

    public async Task<ErrorCode> KillNpcAsync(String email, Int32 stageId, Int32 npcId)
    {
        try
        {
            var errorCode = ErrorCode.NpcNotExist;

            var key = MemoryDbKeyMaker.MakePlayerStageKillingNpcKey(email, stageId);
            var redis = new RedisList<RedisNpcDTO>(_redisConn, key, StageKeyTimeSpan());

            var npcs = await redis.RangeAsync(0, -1);
            for (var idx = 0; idx < npcs.Length; idx++)
            {
                var npc = npcs[idx];
                if (npc.NpcId == npcId)
                {
                    if (npc.CurCount >= npc.TotalCount)
                    {
                        return ErrorCode.OverTotalNpcCount;
                    }

                    npc.CurCount++;
                    npcs[idx] = npc;

                    errorCode = ErrorCode.None;
                }
            }

            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            await redis.DeleteAsync();
            await redis.RightPushAsync(npcs);

            s_logger.ZLogDebug(EventIdDic[EventType.KillNpc],
                $"RedisService.KillNpcAsync: Key = {key}, Message = Success Kill Npc number {npcId}!");

            return errorCode;
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.KillNpc], ex,
                $"RedisDb.ItemFarming: stageId = {stageId}, npcId = {npcId}, ErrorMessage = KillNpc Exception, ErrorCode: {ErrorCode.KillNpcException}");
            return ErrorCode.KillNpcException;
        }
    }

    private async Task<(ErrorCode, List<RedisItemDTO>)> GetFarmedItemsAndDeleteAsync(String email, Int32 stageId)
    {
        try
        {
            var key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(email, stageId);
            var redis = new RedisList<RedisItemDTO>(_redisConn, key, StageKeyTimeSpan());

            var result = await redis.RangeAsync(0, -1);
            await redis.DeleteAsync();

            return (ErrorCode.None, result.ToList());
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.RedisService], ex,
                $"MemoryService.GetFarmedItems: email = {email}, stageId = {stageId}, ErrorMessage = KillNpc Exception, ErrorCode: {ErrorCode.GetFarmedItemsException}");
            return (ErrorCode.GetFarmedItemsException, null);
        }
    }

    public async Task<ErrorCode> EnterStageAsync(String email, Int32 stageId)
    {
        try
        {
            (bool isExisting, AuthUser user) = await GetUserAsync(email);
            if (!isExisting)
            {
                return ErrorCode.AuthUserNotExist;
            }

            if (user.State == UserState.Playing.ToString() || !await SetUserStateAsync(user, UserState.Playing))
            {
                return ErrorCode.EnterStageFail;
            }

            // 던전 NPC 정보 로딩
            var key = MemoryDbKeyMaker.MakePlayerStageKillingNpcKey(email, stageId);
            var npcRedis = new RedisList<RedisNpcDTO>(_redisConn, key, StageKeyTimeSpan());
            var npcs = _memoryCacheService.GetAttackNpcsByStageId(stageId);
            foreach (var npc in npcs.Item2)
            {
                var redisNpc = new RedisNpcDTO
                {
                    NpcId = npc.NpcId,
                    CurCount = 0,
                    TotalCount = npc.NpcCount
                };

                await npcRedis.RightPushAsync(redisNpc);
            }

            // 던전 아이템 정보 로딩
            key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(email, stageId);
            var farmingRedis = new RedisList<RedisItemDTO>(_redisConn, key, StageKeyTimeSpan());
            var items = _memoryCacheService.GetStageItemsByStageId(stageId);
            foreach (var item in items.Item2)
            {
                var redisItem = new RedisItemDTO
                {
                    ItemId = item,
                    CurCount = 0,
                    TotalCount = 1
                };

                await farmingRedis.RightPushAsync(redisItem);
            }

            return ErrorCode.None;
        }
        catch
        {
            // 롤백
            AuthUser user = GetUserAsync(email).Result.Item2;

            await SetUserStateAsync(user, UserState.Login);

            var key = MemoryDbKeyMaker.MakePlayerStageKillingNpcKey(email, stageId);
            var npcRedis = new RedisList<RedisNpcDTO>(_redisConn, key, StageKeyTimeSpan());
            await npcRedis.DeleteAsync();

            key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(email, stageId);
            var farmingRedis = new RedisList<RedisItemDTO>(_redisConn, key, StageKeyTimeSpan());
            await farmingRedis.DeleteAsync();

            return ErrorCode.MemoryEnterStageAsyncException;
        }
    }

    public async Task<ErrorCode> IsPlaying(String email)
    {
        try
        {
            (bool isExisting, AuthUser user) = await GetUserAsync(email);
            if (!isExisting)
            {
                return ErrorCode.AuthUserNotExist;
            }

            if (user.State != UserState.Playing.ToString())
            {
                return ErrorCode.UserStateIsNotPlaying;
            }

            return ErrorCode.None;
        }
        catch
        {
            return ErrorCode.IsPlayingException;
        }
    }

    public async Task<(ErrorCode, List<RedisItemDTO>)> CompleteStage(String email, Int32 stageId)
    {
        try
        {
            var key = MemoryDbKeyMaker.MakePlayerStageKillingNpcKey(email, stageId);
            var redis = new RedisList<RedisNpcDTO>(_redisConn, key, StageKeyTimeSpan());

            var npcs = await redis.RangeAsync(0, -1);
            for (var idx = 0; idx < npcs.Length; idx++)
            {
                var npc = npcs[idx];

                if (npc.CurCount < npc.TotalCount)
                {
                    return (ErrorCode.StageNpcExist, null);
                }
            }

            (bool isExisting, AuthUser user) = await GetUserAsync(email);
            if (!isExisting)
            {
                return (ErrorCode.AuthUserNotExist, null);
            }

            if (!await SetUserStateAsync(user, UserState.Login))
            {
                return (ErrorCode.SetUserStateFail, null);
            }

            var (errorCode, items) = await GetFarmedItemsAndDeleteAsync(email, stageId);
            if (errorCode != ErrorCode.None)
            {
                // 롤백
                await SetUserStateAsync(user, UserState.Playing);
                return (errorCode, null);
            }

            if (!await redis.DeleteAsync())
            {
                // 롤백
                key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(email, stageId);
                var farmingRedis = new RedisList<RedisItemDTO>(_redisConn, key, StageKeyTimeSpan());
                await farmingRedis.RightPushAsync(items);
            }

            s_logger.ZLogDebug(EventIdDic[EventType.CompleteStage],
                $"RedisService.CompleteStage: Key = {key}, Message = Success Clear Stage Number {stageId}!");

            return (ErrorCode.None, items);
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.StopStage], ex,
                $"RedisService.CompleteStage: stageId = {stageId}, ErrorMessage = Complete Stage Exception, ErrorCode: {ErrorCode.CompleteStageException}");
            return (ErrorCode.CompleteStageException, null);
        }
    }

    public async Task<ErrorCode> StopStage(String email, Int32 stageId)
    {
        try
        {
            var key = MemoryDbKeyMaker.MakePlayerStageKillingNpcKey(email, stageId);
            var npcRedis = new RedisList<RedisNpcDTO>(_redisConn, key, StageKeyTimeSpan());
            var npcs = await npcRedis.RangeAsync(0, -1);
            if (!await npcRedis.DeleteAsync())
            {
                return ErrorCode.DeleteNpcRedisFail;
            }

            key = MemoryDbKeyMaker.MakePlayerStageFarmingKey(email, stageId);
            var farmingRedis = new RedisList<RedisItemDTO>(_redisConn, key, StageKeyTimeSpan());
            if (!await farmingRedis.DeleteAsync())
            {
                // 롤백
                await npcRedis.RightPushAsync(npcs);

                return ErrorCode.DeleteFarmingRedisFail;
            }

            s_logger.ZLogDebug(EventIdDic[EventType.StopStage],
                $"RedisService.StopStage: Key = {key}, Message = Success Stop Stage Number {stageId}!");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            s_logger.ZLogError(EventIdDic[EventType.StopStage], ex,
                $"RedisService.StopStage: stageId = {stageId}, ErrorMessage = Complete Stage Exception, ErrorCode: {ErrorCode.StopStageException}");
            return ErrorCode.StopStageException;
        }
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