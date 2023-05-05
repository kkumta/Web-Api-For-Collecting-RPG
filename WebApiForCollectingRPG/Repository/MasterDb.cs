using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApiForCollectingRPG.DAO;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.Services;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Repository;

public class MasterDb : IMasterDb
{
    readonly ILogger<MasterDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    readonly IMemoryCache _cache;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove)
                .SetAbsoluteExpiration(DateTime.MaxValue);

    public MasterDb(ILogger<MasterDb> logger, IOptions<DbConfig> dbConfig, IMemoryCache memoryCache)
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

    public async void GetItemList()
    {
        try
        {
            string key = "item_list";

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
            _logger.ZLogDebug(EventIdDic[EventType.MasterDb],
                $"[MasterDb.GetItemList] item_list: {_cache.Get("item_list")}");

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterDb], ex,
                $"[MasterDb.GetItemList] ErrorCode : {ErrorCode.GetItemListFail}");
        }
    }

    public async void GetItemAttributeList()
    {
        try
        {
            string key = "item_attribute_list";

            var itemAttributeList = await _queryFactory.Query("item_attribute")
                .Select("attribute_id AS AttributeId",
                "name AS Name")
                .GetAsync<ItemAttribute>();

            _cache.Set(key, itemAttributeList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterDb],
                $"[MasterDb.GetItemAttributeList] item_attribute_list: {_cache.Get("item_attribute_list")}");

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterDb], ex,
                $"[MasterDb.GetItemAttributeList] ErrorCode : {ErrorCode.GetItemAttributeListFail}");
        }
    }

    public async void GetAttendanceCompensation()
    {
        try
        {
            string key = "attendance_compensation";

            var attendanceCompensation = await _queryFactory.Query("attendance_compensation")
                .Select("compensation_id AS CompensationId", "item_id AS ItemId", "item_count AS ItemCount")
                .GetAsync<AttendanceCompensation>();

            _cache.Set(key, attendanceCompensation, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterDb],
                $"[MasterDb.GetAttendanceCompensation] attendance_compensation: {_cache.Get("attendance_compensation")}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterDb], ex,
                $"[MasterDb.GetAttendanceCompensation] ErrorCode : {ErrorCode.GetAttendanceCompensationFail}");
        }
    }

    public async void GetInAppProductListAsync()
    {
        try
        {
            string key = "in_app_product_list";

            var inAppProductList = await _queryFactory.Query("in_app_product")
                .Select("product_id AS ProductId", "item_id AS ItemId", "item_name AS ItemName", "item_count AS ItemCount")
                .GetAsync<InAppProduct>();

            _cache.Set(key, inAppProductList, cacheOptions);
            _logger.ZLogDebug(EventIdDic[EventType.MasterDb],
                $"[MasterDb.GetInAppProductListAsync] in_app_product_list: {_cache.Get("in_app_product_list")}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterDb], ex,
                $"[MasterDb.GetInAppProductListAsync] ErrorCode : {ErrorCode.GetInAppProductListFail}");
        }
    }

    public AttendanceCompensation GetAttendanceCompensationByCompensationId(short compensationId)
    {
        try
        {
            List<AttendanceCompensation> attendanceCompensationlist = _cache.Get("attendance_compensation") as List<AttendanceCompensation>;
            return attendanceCompensationlist.First(x => x.CompensationId == compensationId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.MasterDb], ex,
                $"[MasterDb.GetAttendanceCompensationByCompensationId] ErrorCode: {ErrorCode.GetAttendanceCompensationExeption}, CompensationId: {compensationId}");
            return new AttendanceCompensation();
        }
    }
}