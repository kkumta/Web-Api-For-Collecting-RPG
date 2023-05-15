using System;

namespace WebApiForCollectingRPG.DAO.Master;

public class StageAttackNpc
{
    public Int64 StageAttackNpcId { get; set; }
    public Int32 StageId { get; set; }
    public Int32 NpcId { get; set; }
    public Int32 NpcCount { get; set; }
    public Int64 Exp { get; set; }
}