using System;

namespace WebApiForCollectingRPG.DAO;

public class AccountItem
{
    public Int64 AccountItemId { get; set; }
    public Int64 AccountId { get; set; }
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }
    public Int16 EnhanceCount { get; set; }
    public Int64 Attack { get; set; }
    public Int64 Defence { get; set; }
    public Int64 Magic { get; set; }
}