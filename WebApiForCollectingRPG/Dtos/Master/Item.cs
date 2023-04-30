using System;

namespace WebApiForCollectingRPG.Dtos.MasterData;

public class Item
{
    public Int64 ItemId { get; set; }
    public String Name { get; set; }
    public Int32 AttributeId { get; set; }
    public Int64 SellPrice { get; set; }
    public Int64 BuyPrice { get; set; }
    public Int16 UseLv { get; set; }
    public Int64 Attack { get; set; }
    public Int64 Defence { get; set; }
    public Int64 Magic { get; set; }
    public Int64 EnhanceMaxCount { get; set; }
    public bool IsItemStackable { get; set; }
}
