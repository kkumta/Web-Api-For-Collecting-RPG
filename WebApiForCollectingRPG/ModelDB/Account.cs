﻿using System;

namespace WebApiForCollectingRPG.ModelDB;

public class Account
{
    public Int64 AccountId { get; set; }
    public String Email { get; set; }
    public String HashedPassword { get; set; }
    public String SaltValue { get; set; }
}

public class AuthUser
{
    public String Email { get; set; } = "";
    public String AuthToken { get; set; } = "";
    public Int64 AccountId { get; set; } = 0;
    public String State { get; set; } = "";
}

public enum UserState
{
    Default = 0,
    Login = 1,
    Playing = 2
}

public class RedisKeyExpireTime
{
    public const ushort NxKeyExpireSecond = 3;
    public const ushort RegistKeyExpireSecond = 6000;
    public const ushort LoginKeyExpireMin = 60;
    public const ushort TicketKeyExpireSecond = 6000; // 현재 테스트를 위해 티켓은 10분동안 삭제하지 않는다. 
}