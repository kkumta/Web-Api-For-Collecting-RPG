using System.Collections.Generic;
using System;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.Dungeon;
using WebApiForCollectingRPG.DTO.InAppProduct;

namespace WebApiForCollectingRPG.Services;

public interface IMemoryCacheService
{
    public AttendanceCompensation GetAttendanceCompensationByCompensationId(Int16 compensationId);
    public List<InAppItemDTO> GetInAppItemsByProductId(Int16 productId);
    public bool IsMoney(Int64 itemId);
    public bool IsStackableItem(Int64 itemId);
    public Item GetItemByItemId(Int64 itemId);
    public Int32 GetTotalStageCount();
    public Tuple<ErrorCode, List<Int64>> GetStageItemsByStageId(Int32 stageId);
    public Tuple<ErrorCode, List<AttackNpcDTO>> GetAttackNpcsByStageId(Int32 stageId);
    public Tuple<ErrorCode, Int64> GetAttackNpcExpByNpcId(Int32 npcId);
    public Tuple<ErrorCode, Int16> GetAttendanceSize();
}