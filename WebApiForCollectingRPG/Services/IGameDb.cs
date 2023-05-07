using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.DTO.Game;

namespace WebApiForCollectingRPG.Services;

public interface IGameDb : IDisposable
{
    public Task<ErrorCode> CreateAccountGameDataAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, AccountGameInfo>> GetAccountGameInfoAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, IEnumerable<AccountItemInfo>>> GetAccoutItemInfoListAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, IEnumerable<MailListInfo>>> GetMailsByPage(Int64 accountId, Int32 page);
    public Task<Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>> GetMailByMailId(Int64 accountId, Int64 mailId);
    public Task<ErrorCode> CheckAttendance(Int64 accountId);
    public Task<ErrorCode> SendInAppProduct(Int64 accountId, Int64 receiptId, Int16 productId);
    public Task<ErrorCode> ReceiveMailItems(Int64 accountId, Int64 mailId);
}