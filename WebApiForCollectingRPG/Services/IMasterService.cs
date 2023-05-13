using System;
using System.Collections.Generic;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.InAppProduct;

namespace WebApiForCollectingRPG.Services;

public interface IMasterService
{
    public void LoadItemList();
    public void LoadItemAttributeList();
    public void LoadAttendanceCompensation();
    public void LoadInAppProductListAsync();
    public AttendanceCompensation GetAttendanceCompensationByCompensationId(Int16 compensationId);
    public List<InAppItemDTO> GetInAppItemsByProductId(Int16 productId);
    public bool IsMoney(Int64 itemId);
    public bool IsStackableItem(Int64 itemId);
    public Item GetItemByItemId(Int64 itemId);
}