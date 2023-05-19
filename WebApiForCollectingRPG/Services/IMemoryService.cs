using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;
using WebApiForCollectingRPG.DTO.Dungeon;

namespace WebApiForCollectingRPG.Services;

public interface IMemoryService
{
    public void Init(String address);
    public Task<ErrorCode> RegistUserAsync(String email, String authToken, Int64 accountId, Int64 playerId);
    public Task<ErrorCode> CheckUserAuthAsync(String email, String authToken);
    public Task<(bool, AuthUser)> GetUserAsync(String email);
    public Task<bool> SetUserStateAsync(AuthUser user, UserState userState);
    public Task<bool> SetUserReqLockAsync(String key);
    public Task<bool> DelUserReqLockAsync(String key);
    public Task<(ErrorCode, Notice)> GetNoticeAsync();
    public Task<ErrorCode> ItemFarmingAsync(String email, Int32 stageId, Int64 itemId);
    public Task<ErrorCode> KillNpcAsync(String email, Int32 stageId, Int32 npcId);
    public Task<ErrorCode> EnterStageAsync(String email, Int32 stageId);
    public Task<ErrorCode> IsPlaying(String email);
    public Task<(ErrorCode, List<RedisItemDTO>)> CompleteStage(String email, Int32 stageId);
    public Task<ErrorCode> StopStage(String email, Int32 stageId);
}