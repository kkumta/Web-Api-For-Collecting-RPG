using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

public class LogManager
{
    public enum EventType
    {
        CreateAccount = 101,
        Login = 201,
        LoginAddRedis = 202,
        ReceiveNotice = 301,
        GetMail = 401,
        GetMails = 402,
        ReceiveMailItems = 403,
        GetAttendance = 501,
        CheckAttendance = 502,
        ReceiveInAppProduct = 601,
        EnhanceItem = 701,
        GetAllStages = 801,
        EnterStage = 802,
        ItemFarming = 803,
        KillNpc = 804,
        AccountService = 1001,
        GameService = 1002,
        MasterService = 1003,
        MemoryCacheService = 1004,
        RedisService = 1005,
        AccountRepository = 2001,
    }

    private static ILoggerFactory s_loggerFactory;

    public static Dictionary<EventType, EventId> EventIdDic = new()
    {
        {EventType.CreateAccount, new EventId((int)EventType.CreateAccount, "CreateAccount") },
        {EventType.Login, new EventId((int)EventType.Login, "Login") },
        {EventType.LoginAddRedis, new EventId((int)EventType.LoginAddRedis, "LoginAddRedis") },
        {EventType.ReceiveNotice, new EventId((int)EventType.ReceiveNotice, "ReceiveNotice") },
        {EventType.GetMail, new EventId((int)EventType.GetMail, "GetMail") },
        {EventType.GetMails, new EventId((int)EventType.GetMails, "GetMails") },
        {EventType.ReceiveMailItems, new EventId((int)EventType.ReceiveMailItems, "ReceiveMailItems") },
        {EventType.GetAttendance, new EventId((int)EventType.GetAttendance, "GetAttendance") },
        {EventType.CheckAttendance, new EventId((int)EventType.CheckAttendance, "CheckAttendance") },
        {EventType.ReceiveInAppProduct, new EventId((int)EventType.ReceiveInAppProduct, "ReceiveInAppProduct") },
        {EventType.EnhanceItem, new EventId((int)EventType.EnhanceItem, "EnhanceItem") },
        {EventType.GetAllStages, new EventId((int)EventType.GetAllStages, "GetAllStages") },
        {EventType.EnterStage, new EventId((int)EventType.EnterStage, "EnterStage") },
        {EventType.ItemFarming, new EventId((int)EventType.ItemFarming, "ItemFarming") },
        {EventType.KillNpc, new EventId((int)EventType.KillNpc, "KillNpc") },
        {EventType.AccountService, new EventId((int)EventType.AccountService, "AccountService") },
        {EventType.GameService, new EventId((int)EventType.GameService, "GameService") },
        {EventType.MasterService, new EventId((int)EventType.MasterService, "MasterService") },
        {EventType.MemoryCacheService, new EventId((int)EventType.MemoryCacheService, "MemoryCacheService") },
        {EventType.RedisService, new EventId((int)EventType.RedisService, "RedisService") },
        {EventType.AccountRepository, new EventId((int)EventType.AccountRepository, "AccountRepository") }
    };

    public static ILogger Logger { get; private set; }

    public static void SetLoggerFactory(ILoggerFactory loggerFactory, String categoryName)
    {
        s_loggerFactory = loggerFactory;
        Logger = loggerFactory.CreateLogger(categoryName);
    }

    public static ILogger<T> GetLogger<T>() where T : class
    {
        return s_loggerFactory.CreateLogger<T>();
    }
}