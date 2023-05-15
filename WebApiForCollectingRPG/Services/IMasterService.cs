using System;
using System.Collections.Generic;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.InAppProduct;

namespace WebApiForCollectingRPG.Services;

public interface IMasterService
{
    public void LoadItemListAsync();
    public void LoadItemAttributeListAsync();
    public void LoadAttendanceCompensationAsync();
    public void LoadInAppProductListAsync();
    public void LoadStageItemListAsync();
    public void LoadStageAttackNpcListAsync();
    public AttendanceCompensation GetAttendanceCompensationByCompensationId(Int16 compensationId);
    public List<InAppItemDTO> GetInAppItemsByProductId(Int16 productId);
    public bool IsMoney(Int64 itemId);
    public bool IsStackableItem(Int64 itemId);
    public Item GetItemByItemId(Int64 itemId);
}