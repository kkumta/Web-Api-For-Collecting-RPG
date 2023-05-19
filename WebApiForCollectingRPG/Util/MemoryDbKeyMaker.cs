using System;

namespace WebApiForCollectingRPG.Util;

public class MemoryDbKeyMaker
{
    const String loginUID = "UID_";
    const String userLockKey = "ULock_";
    const String noticeKey = "Notice_";
    const String playerStageFarmingKey = "Farming_";
    const String playerStageKillingNpcKey = "Killing_";

    public static String MakeUIDKey(String id)
    {
        return loginUID + id;
    }

    public static String MakeUserLockKey(String id)
    {
        return userLockKey + id;
    }

    public static String MakeNoticeKey(Int32 id)
    {
        return noticeKey + id;
    }

    public static String MakePlayerStageFarmingKey(String email, Int32 stageId)
    {
        return playerStageFarmingKey + email + "_" + stageId;
    }

    public static String MakePlayerStageKillingNpcKey(String email, Int32 stageId)
    {
        return playerStageKillingNpcKey + email + "_" + stageId;
    }
}