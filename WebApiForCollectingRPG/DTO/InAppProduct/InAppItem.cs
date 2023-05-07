using System;

namespace WebApiForCollectingRPG.DTO.InAppProduct;

public class InAppItem
{
    public Int64 ItemId { get; set; } = 0;
    public String ItemName { get; set; } = "";
    public Int32 ItemCount { get; set; } = 0;
}