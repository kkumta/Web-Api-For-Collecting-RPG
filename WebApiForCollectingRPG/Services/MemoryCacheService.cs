using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.Dungeon;
using WebApiForCollectingRPG.DTO.InAppProduct;
using static LogManager;
using ZLogger;
using System.Linq;

namespace WebApiForCollectingRPG.Services;

public class MemoryCacheService : IMemoryCacheService
{
    readonly ILogger<MemoryCacheService> _logger;
    readonly IMemoryCache _cache;

    public MemoryCacheService(ILogger<MemoryCacheService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public AttendanceCompensation GetAttendanceCompensationByCompensationId(Int16 compensationId)
    {
        try
        {
            List<AttendanceCompensation> attendanceCompensationlist = _cache.Get("attendance_compensation") as List<AttendanceCompensation>;
            return attendanceCompensationlist.First(x => x.CompensationId == compensationId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetAttendanceCompensationByCompensationId] ErrorCode: {ErrorCode.GetAttendanceCompensationExeption}, CompensationId: {compensationId}");
            return new AttendanceCompensation();
        }
    }

    public List<InAppItemDTO> GetInAppItemsByProductId(Int16 productId)
    {
        try
        {
            List<InAppProduct> itemList = _cache.Get("in_app_product_list") as List<InAppProduct>;
            var seletedItemList = itemList.FindAll(x => x.ProductId == productId);
            return seletedItemList.ConvertAll(product =>
            {
                return new InAppItemDTO()
                {
                    ItemId = product.ItemId,
                    ItemName = product.ItemName,
                    ItemCount = product.ItemCount
                };
            });
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetInAppItemsByProductId] ErrorCode: {ErrorCode.GetInAppItemsByProductIdExeption}, ProductId: {productId}");
            return new List<InAppItemDTO>();
        }
    }

    public bool IsMoney(Int64 itemId)
    {
        try
        {
            List<Item> itemList = _cache.Get("item_list") as List<Item>;
            return itemId == itemList.First(x => x.Name.Equals("돈")).ItemId;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.IsMoney] ErrorCode: {ErrorCode.IsMoneyException}, ItemId: {itemId}");
            return new bool();
        }
    }

    public bool IsStackableItem(Int64 itemId)
    {
        try
        {
            List<Item> itemList = _cache.Get("item_list") as List<Item>;
            return itemList.First(x => x.ItemId == itemId).IsItemStackable;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.IsMoney] ErrorCode: {ErrorCode.IsStackableItemException}, ItemId: {itemId}");
            return new bool();
        }
    }

    public Item GetItemByItemId(Int64 itemId)
    {
        try
        {
            List<Item> itemList = _cache.Get("item_list") as List<Item>;
            return itemList.First(x => x.ItemId == itemId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetItemByItemId] ErrorCode: {ErrorCode.GetItemByItemIdException}, ItemId: {itemId}");
            return new Item();
        }
    }

    public Int32 GetTotalStageCount()
    {
        try
        {
            String key = "total_stage_count";
            return (Int32)_cache.Get(key);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetTotalStageCount] ErrorCode: {ErrorCode.GetTotalStageCountException}");
            return 0;
        }
    }

    public Tuple<ErrorCode, List<Int64>> GetStageItemsByStageId(Int32 stageId)
    {
        try
        {
            var key = "stage_item_list";

            var items = _cache.Get(key) as List<StageItemDTO>;
            var seletedItems = items.FindAll(x => x.StageId == stageId);

            return new Tuple<ErrorCode, List<Int64>>(ErrorCode.None, seletedItems.ConvertAll(item => item.ItemId));
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetStageItemsByStageId] ErrorCode: {ErrorCode.GetStageItemsByStageIdException}, StageId: {stageId}");
            return new Tuple<ErrorCode, List<Int64>>(ErrorCode.GetStageItemsByStageIdException, null);
        }
    }

    public Tuple<ErrorCode, List<AttackNpcDTO>> GetAttackNpcsByStageId(Int32 stageId)
    {
        try
        {
            var key = "stage_attack_npc_list";
            var npcs = _cache.Get(key) as List<StageAttackNpcDTO>;
            var seletedNpcs = npcs.FindAll(x => x.StageId == stageId);

            var convertedNpcs = seletedNpcs.ConvertAll(npc =>
            {
                return new AttackNpcDTO()
                {
                    NpcId = npc.NpcId,
                    NpcCount = npc.NpcCount
                };
            });

            return new Tuple<ErrorCode, List<AttackNpcDTO>>(ErrorCode.None, convertedNpcs);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetAttackNpcsByStageId] ErrorCode: {ErrorCode.GetAttackNpcsByStageIdException}, StageId: {stageId}");
            return new Tuple<ErrorCode, List<AttackNpcDTO>>(ErrorCode.GetAttackNpcsByStageIdException, null);
        }
    }

    public Tuple<ErrorCode, Int64> GetAttackNpcExpByNpcId(Int32 npcId)
    {
        try
        {
            var key = "stage_attack_npc_list";
            var npcs = _cache.Get(key) as List<StageAttackNpcDTO>;
            var seletedNpc = npcs.FirstOrDefault(x => x.NpcId == npcId);

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, seletedNpc.Exp);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetAttackNpcExpByNpcId] ErrorCode: {ErrorCode.GetAttackNpcExpByNpcIdException}, NpcId: {npcId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.GetAttackNpcExpByNpcIdException, 0);
        }
    }

    public Tuple<ErrorCode, Int16> GetAttendanceSize()
    {
        try
        {
            List<AttendanceCompensation> attendanceCompensations = _cache.Get("attendance_compensation") as List<AttendanceCompensation>;
            return new Tuple<ErrorCode, Int16>(ErrorCode.None, (short)attendanceCompensations.Count);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MemoryCacheService], ex,
                $"[MemoryCacheService.GetAttendanceSize] ErrorCode: {ErrorCode.GetAttendanceSizeException}");
            return new Tuple<ErrorCode, Int16>(ErrorCode.GetAttendanceSizeException, 0);
        }
    }
}