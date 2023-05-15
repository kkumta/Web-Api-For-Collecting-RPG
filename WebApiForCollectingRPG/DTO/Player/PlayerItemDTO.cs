using System;

namespace WebApiForCollectingRPG.DTO.Player;

public class PlayerItemDTO
{
    public Int64 PlayerItemId { get; set; }
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }
    public Int16 EnhanceCount { get; set; }
    public Int64 Attack { get; set; }
    public Int64 Defence { get; set; }
    public Int64 Magic { get; set; }
}