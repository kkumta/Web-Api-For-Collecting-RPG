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
        CheckAttendance = 501,
        AccountDb = 1001,
        GameDb = 1002,
        MasterDb = 1003,
        RedisDb = 1004
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
        {EventType.CheckAttendance, new EventId((int)EventType.CheckAttendance, "CheckAttendance") },
        {EventType.AccountDb, new EventId((int)EventType.AccountDb, "AccountDb") },
        {EventType.GameDb, new EventId((int)EventType.GameDb, "GameDb") },
        {EventType.MasterDb, new EventId((int)EventType.MasterDb, "MasterDb") },
        {EventType.RedisDb, new EventId((int)EventType.RedisDb, "RedisDb") }

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