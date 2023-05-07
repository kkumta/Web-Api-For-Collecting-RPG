using System;

namespace WebApiForCollectingRPG.DTO.Game;

public class AccountItemInfo
{
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }
    public Int16 EnhanceCount { get; set; }
}