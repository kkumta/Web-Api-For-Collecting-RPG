using System;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class StageAttackNpcDTO
{
    public Int32 StageId { get; set; }
    public Int32 NpcId { get; set; }
    public Int32 NpcCount { get; set; }
    public Int64 Exp { get; set; }
}