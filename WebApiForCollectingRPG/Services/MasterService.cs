using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.Dungeon;
using WebApiForCollectingRPG.DTO.InAppProduct;
using WebApiForCollectingRPG.Repository;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Services;

public class MasterService : IMasterService
{
    readonly ILogger<MasterService> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    readonly IMemoryCache _cache;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove)
                .SetAbsoluteExpiration(DateTime.MaxValue);

    public MasterService(ILogger<MasterService> logger, IOptions<DbConfig> dbConfig, IMemoryCache memoryCache)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        _cache = memoryCache;

        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConn, _compiler);
    }

    public void Dispose()
    {
        Close();
    }

    private void Open()
    {
        _dbConn = new MySqlConnection(_dbConfig.Value.MasterDb);

        _dbConn.Open();
    }

    private void Close()
    {
        _dbConn.Close();
    }

    public async void LoadItemListAsync()
    {
        try
        {
            String key = "item_list";

            var itemList = await _queryFactory.Query("item")
                .Select("item_id AS ItemId",
                "name AS Name",
                "attribute_id AS AttributeId",
                "sell_price AS SellPrice",
                "buy_price AS BuyPrice",
                "use_lv AS UseLv",
                "attack AS Attack",
                "defence AS Defence",
                "magic AS Magic",
                "enhance_max_count AS EnhanceMaxCount",
                "is_item_stackable AS IsItemStackable")
                .GetAsync<Item>();

            _cache.Set(key, itemList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadItemList] item_list: {_cache.Get("item_list")}");

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadItemList] ErrorCode : {ErrorCode.LoadItemListFail}");
        }
    }

    public async void LoadItemAttributeListAsync()
    {
        try
        {
            String key = "item_attribute_list";

            var itemAttributeList = await _queryFactory.Query("item_attribute")
                .Select("attribute_id AS AttributeId",
                "name AS Name")
                .GetAsync<ItemAttribute>();

            _cache.Set(key, itemAttributeList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadItemAttributeList] item_attribute_list: {_cache.Get(key)}");

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadItemAttributeList] ErrorCode : {ErrorCode.LoadItemAttributeListFail}");
        }
    }

    public async void LoadAttendanceCompensationAsync()
    {
        try
        {
            String key = "attendance_compensation";

            var attendanceCompensation = await _queryFactory.Query("attendance_compensation")
                .Select("compensation_id AS CompensationId", "item_id AS ItemId", "item_count AS ItemCount")
                .GetAsync<AttendanceCompensation>();

            _cache.Set(key, attendanceCompensation, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadAttendanceCompensation] attendance_compensation: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadAttendanceCompensation] ErrorCode : {ErrorCode.LoadAttendanceCompensationFail}");
        }
    }

    public async void LoadInAppProductListAsync()
    {
        try
        {
            String key = "in_app_product_list";

            var inAppProductList = await _queryFactory.Query("in_app_product")
                .Select("product_id AS ProductId", "item_id AS ItemId", "item_name AS ItemName", "item_count AS ItemCount")
                .GetAsync<InAppProduct>();

            _cache.Set(key, inAppProductList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadInAppProductListAsync] in_app_product_list: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadInAppProductListAsync] ErrorCode : {ErrorCode.LoadInAppProductListFail}");
        }
    }

    public async void LoadStageItemListAsync()
    {
        try
        {
            String key = "stage_item_list";

            var stageItemList = await _queryFactory.Query("stage_item")
                .Select("stage_id AS StageId", "item_id AS ItemId")
                .GetAsync<StageItemDTO>();

            _cache.Set(key, stageItemList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadStageItemListAsync] stage_item_list: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadStageItemListAsync] ErrorCode : {ErrorCode.LoadStageItemListAsyncException}");
        }
    }

    public async void LoadStageAttackNpcListAsync()
    {
        try
        {
            String key = "stage_attack_npc_list";

            var stageAttackNpcList = await _queryFactory.Query("stage_attack_npc")
                .Select("stage_id AS StageId", "npc_id AS NpcId", "npc_count AS NpcCount", "exp AS Exp")
                .GetAsync<StageAttackNpcDTO>();

            _cache.Set(key, stageAttackNpcList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadStageAttackNpcListAsync] stage_attack_npc_list: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadStageAttackNpcListAsync] ErrorCode : {ErrorCode.LoadStageAttackNpcListAsyncException}");
        }
    }

    public async void LoadTotalStageCountAsync()
    {
        try
        {
            String key = "total_stage_count";

            var stageCount = await _queryFactory.Query("stage_item")
                .SelectRaw("COUNT(DISTINCT stage_id)")
                .FirstOrDefaultAsync<Int32>();

            _cache.Set(key, stageCount, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadTotalStageCountAsync] total_stage_count: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadTotalStageCountAsync] ErrorCode : {ErrorCode.LoadTotalStageCountAsyncException}");
        }
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
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.GetAttendanceCompensationByCompensationId] ErrorCode: {ErrorCode.GetAttendanceCompensationExeption}, CompensationId: {compensationId}");
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
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.GetInAppItemsByProductId] ErrorCode: {ErrorCode.GetInAppItemsByProductIdExeption}, ProductId: {productId}");
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
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.IsMoney] ErrorCode: {ErrorCode.IsMoneyException}, ItemId: {itemId}");
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
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.IsMoney] ErrorCode: {ErrorCode.IsStackableItemException}, ItemId: {itemId}");
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
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.GetItemByItemId] ErrorCode: {ErrorCode.GetItemByItemIdException}, ItemId: {itemId}");
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
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.GetTotalStageCount] ErrorCode: {ErrorCode.GetTotalStageCountException}");
            return 0;
        }
    }
}