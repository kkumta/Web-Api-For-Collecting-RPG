using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.Dtos.Game;

namespace WebApiForCollectingRPG.Services;

public interface IGameDb : IDisposable
{
    public Task<ErrorCode> CreateAccountGameDataAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, AccountGame>> GetAccountGameInfoAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, IEnumerable<AccountItem>>> GetAccountItemListAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, IEnumerable<MailListInfo>>> GetMailsByPage(Int64 accountId, Int32 page);
    public Task<Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>> GetMailByMailId(Int64 accountId, Int64 mailId);
}