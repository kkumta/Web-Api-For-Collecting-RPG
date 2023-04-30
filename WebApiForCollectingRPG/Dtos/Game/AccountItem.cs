using System;

namespace WebApiForCollectingRPG.Dtos.Game;

public class AccountItem
{
    public Int16 SlotId { get; set; }
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }
    public Int16 EnhanceCount { get; set; }
}