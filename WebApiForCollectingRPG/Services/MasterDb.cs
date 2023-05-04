using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Data;
using WebApiForCollectingRPG.Dtos.MasterData;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Services;

public class MasterDb : IMasterDb
{
    readonly ILogger<MasterDb> _logger;
    readonly IOptions<DbConfig> _dbConfig;
    readonly IMemoryCache _cache;

    IDbConnection _dbConn;
    SqlKata.Compilers.MySqlCompiler _compiler;
    QueryFactory _queryFactory;

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


            var cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove)
                .SetAbsoluteExpiration(DateTime.MaxValue);

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
            String key = "item_attribute_list";

            var itemAttributeList = await _queryFactory.Query("item_attribute")
                .Select("attribute_id AS AttributeId",
                "name AS Name")
                .GetAsync<ItemAttribute>();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove)
                .SetAbsoluteExpiration(DateTime.MaxValue);

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
}