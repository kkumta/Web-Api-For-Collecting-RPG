using System;

namespace WebApiForCollectingRPG.DTO.Player;

public class PlayerStackableItemDTO
{
    public Int64 PlayerItemId { get; set; }
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }
}