using System;

namespace WebApiForCollectingRPG.Services;

public class MemoryDbKeyMaker
{
    const String loginUID = "UID_";
    const String userLockKey = "ULock_";

    public static String MakeUIDKey(String id)
    {
        return loginUID + id;
    }

    public static String MakeUserLockKey(String id)
    {
        return userLockKey + id;
    }
}