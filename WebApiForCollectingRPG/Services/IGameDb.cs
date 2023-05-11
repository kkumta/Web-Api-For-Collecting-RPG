﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.DTO.Game;

namespace WebApiForCollectingRPG.Services;

public interface IGameDb : IDisposable
{
    public Task<Tuple<ErrorCode, Int64>> CreatePlayerAsync(Int64 accountId);
    public Task<Tuple<ErrorCode, Int64>> FindPlayerIdByAccountId(Int64 accountId);
    public Task<ErrorCode> CreatePlayerGameDataAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, PlayerGameInfo>> GetPlayerGameInfoAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, IEnumerable<PlayerItemInfo>>> GetPlayerItemInfoListAsync(Int64 playerId);
    public Task<Tuple<ErrorCode, IEnumerable<MailListInfo>>> GetMailsByPage(Int32 page);
    public Task<Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>> GetMailByMailId(Int64 mailId);
    public Task<ErrorCode> CheckAttendance();
    public Task<ErrorCode> SendInAppProduct(Int64 receiptId, Int16 productId);
    public Task<ErrorCode> ReceiveMailItems(Int64 mailId);
    public Task<Tuple<ErrorCode, bool>> EnhanceItem(Int64 playerItemId);
}