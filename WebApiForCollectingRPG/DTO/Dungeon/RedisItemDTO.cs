using System;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class RedisItemDTO
{
    public Int64 ItemId { get; set; }
    public Int32 CurCount { get; set; }
    public Int32 TotalCount { get; set; }
}