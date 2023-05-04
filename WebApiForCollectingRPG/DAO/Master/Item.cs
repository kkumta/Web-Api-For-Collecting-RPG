using System;

namespace WebApiForCollectingRPG.DAO.Master;

public class Item
{
    public long ItemId { get; set; }
    public string Name { get; set; }
    public int AttributeId { get; set; }
    public long SellPrice { get; set; }
    public long BuyPrice { get; set; }
    public short UseLv { get; set; }
    public long Attack { get; set; }
    public long Defence { get; set; }
    public long Magic { get; set; }
    public long EnhanceMaxCount { get; set; }
    public bool IsItemStackable { get; set; }
}
