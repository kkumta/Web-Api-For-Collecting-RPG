using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.DTO.Player;
using WebApiForCollectingRPG.DTO.Dungeon;
using WebApiForCollectingRPG.DTO.Attendance;

namespace WebApiForCollectingRPG.Services;

public interface IGameService : IDisposable
{
    public Task<Tuple<ErrorCode, Int64>> CreatePlayerAsync(Int64 accountId);
    public Task<ErrorCode> DeletePlayerAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, Int64>> FindPlayerIdByAccountId(Int64 accountId);
    public Task<ErrorCode> CreatePlayerGameDataAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, PlayerGameDTO>> GetPlayerGameInfoAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, IEnumerable<PlayerItemDTO>>> GetPlayerItemInfoListAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, IEnumerable<MailListInfo>>> GetMailsByPage(Int32 page);
    public Task<Tuple<ErrorCode, MailDetail, IEnumerable<ItemDTO>>> GetMailByMailId(Int64 mailId);
    public Task<Tuple<ErrorCode, IEnumerable<AttendanceDetail>>> GetAttendanceAsync();
    public Task<ErrorCode> CheckAttendance();
    public Task<ErrorCode> SendInAppProduct(Int64 receiptId, Int16 productId);
    public Task<ErrorCode> ReceiveMailItems(Int64 mailId);
    public Task<Tuple<ErrorCode, bool>> EnhanceItem(Int64 playerItemId);
    public Task<Tuple<ErrorCode, IEnumerable<StageDetail>>> GetAllStagesAsync();
    public Task<Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>> EnterStageAsync(Int32 requestedStageId);
    public Task<ErrorCode> SaveStageRewardToPlayer(Int32 stageId, List<RedisItemDTO> redisItems);
}