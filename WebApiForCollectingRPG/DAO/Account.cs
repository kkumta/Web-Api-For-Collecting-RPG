using System;

namespace WebApiForCollectingRPG.DAO;

public class Account
{
    public Int64 AccountId { get; set; }
    public String Email { get; set; }
    public String HashedPassword { get; set; }
    public String SaltValue { get; set; }
}

public class RediskeyExpireTime
{
    public const UInt16 NxKeyExpireSecond = 3;
    public const UInt16 RegistKeyExpireSecond = 6000;
    public const UInt16 LoginKeyExpireMin = 60;
    public const UInt16 TicketKeyExpireSecond = 6000; // 현재 테스트를 위해 티켓은 10분동안 삭제하지 않는다. 
    public const UInt16 StageExpireSecond = 3000;
}