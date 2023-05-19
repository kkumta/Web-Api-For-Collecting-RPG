using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Data;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.Dungeon;
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

    public void LoadItemList()
    {
        try
        {
            String key = "item_list";

            var itemList = _queryFactory.Query("item")
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
                .Get<Item>();

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

    public void LoadItemAttributeList()
    {
        try
        {
            String key = "item_attribute_list";

            var itemAttributeList = _queryFactory.Query("item_attribute")
                .Select("attribute_id AS AttributeId",
                "name AS Name")
                .Get<ItemAttribute>();

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

    public void LoadAttendanceCompensation
        ()
    {
        try
        {
            String key = "attendance_compensation";

            var attendanceCompensation = _queryFactory.Query("attendance_compensation")
                .Select("compensation_id AS CompensationId", "item_id AS ItemId", "item_count AS ItemCount")
                .Get<AttendanceCompensation>();

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

    public void LoadInAppProductList()
    {
        try
        {
            String key = "in_app_product_list";

            var inAppProductList = _queryFactory.Query("in_app_product")
                .Select("product_id AS ProductId", "item_id AS ItemId", "item_name AS ItemName", "item_count AS ItemCount")
                .Get<InAppProduct>();

            _cache.Set(key, inAppProductList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadInAppProductList] in_app_product_list: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadInAppProductList] ErrorCode : {ErrorCode.LoadInAppProductListFail}");
        }
    }

    public void LoadStageItemList()
    {
        try
        {
            String key = "stage_item_list";

            var stageItemList = _queryFactory.Query("stage_item")
                .Select("stage_id AS StageId", "item_id AS ItemId")
                .Get<StageItemDTO>();

            _cache.Set(key, stageItemList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadStageItemList] stage_item_list: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadStageItemList] ErrorCode : {ErrorCode.LoadStageItemListException}");
        }
    }

    public void LoadStageAttackNpcList()
    {
        try
        {
            String key = "stage_attack_npc_list";

            var stageAttackNpcList = _queryFactory.Query("stage_attack_npc")
                .Select("stage_id AS StageId", "npc_id AS NpcId", "npc_count AS NpcCount", "exp AS Exp")
                .Get<StageAttackNpcDTO>();

            _cache.Set(key, stageAttackNpcList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadStageAttackNpcList] stage_attack_npc_list: {_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadStageAttackNpcList] ErrorCode : {ErrorCode.LoadStageAttackNpcListException}");
        }
    }

    public void LoadTotalStageCount()
    {
        try
        {
            String key = "total_stage_count";

            var stageCount = _queryFactory.Query("stage_item")
                .SelectRaw("COUNT(DISTINCT stage_id)")
                .FirstOrDefault<Int32>();

            _cache.Set(key, stageCount, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterService],
                $"[MasterService.LoadTotalStageCount] total_stage_count: {(Int32)_cache.Get(key)}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterService], ex,
                $"[MasterService.LoadTotalStageCount] ErrorCode : {ErrorCode.LoadTotalStageCountException}");
        }
    }
}