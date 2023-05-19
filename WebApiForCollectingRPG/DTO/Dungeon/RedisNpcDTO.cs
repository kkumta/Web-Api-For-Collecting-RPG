using System;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class RedisNpcDTO
{
    public Int32 NpcId { get; set; }
    public Int32 CurCount { get; set; }
    public Int32 TotalCount { get; set; }
}