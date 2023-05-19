using System;

public enum ErrorCode : UInt16
{
    None = 0,

    // Common
    UnhandleException = 1001,
    RedisFailException = 1002,
    InValidRequestHttpBody = 1003,
    AuthTokenFailWrongAuthToken = 1006,
    VersionFailWrongKeyword = 1007,

    // Account
    CreateAccountFailException = 2001,
    LoginFailException = 2002,
    LoginFailUserNotExist = 2003,
    LoginFailPasswordNotMatch = 2004,
    LoginFailSetAuthToken = 2005,
    AuthTokenMismatch = 2006,
    AuthTokenNotFound = 2007,
    AuthTokenFailWrongKeyword = 2008,
    AuthTokenFailSetNx = 2009,
    AccountIdMismatch = 2010,
    DuplicatedLogin = 2011,
    CreateAccountFailInsert = 2012,
    LoginFailAddRedis = 2014,
    CheckAuthFailNotExist = 2015,
    CheckAuthFailNotMatch = 2016,
    CheckAuthFailException = 2017,
    GetAccountDbConnectionFail = 2018,
    FindAccountIdByEmailFailNotExist = 2019,
    FindAccountIdByEmailFailException = 2020,
    DeleteAccountException = 2021,

    // GameDb
    GetGameDbConnectionFail = 4001,
    CreatePlayerGameFailException = 4002,
    CreatePlayerItemFailException = 4003,

    // Player
    GetPlayerGameInfoFailNotExist = 4101,
    GetPlayerGameInfoFailException = 4102,
    GetPlayerItemListFailNotExist = 4103,
    GetPlayerItemListFailException = 4104,
    AddPlayerExpAsyncException = 4105,

    // Mail
    GetMailsFailNotExistPage = 4201,
    GetMailsFailException = 4202,
    GetMailFailNotExist = 4203,
    GetMailFailException = 4204,
    SendRewardToMailboxException = 4205,
    ReceiveMailItemsFailNotExist = 4206,
    ReceiveMailItemsFailMailNotExist = 4207,
    ReceiveMailItemsException = 4208,
    ReceiveItemsActionsException = 4209,
    ReceiveItemsActionsRollbackException = 4210,
    SendRewardToMailboxError = 4211,

    // CheckAttendance
    GetAttendanceAsyncException = 4301,
    AttendanceInfoNotExist = 4302,
    CheckAttendanceException = 4303,
    DuplicateAttendance = 4304,

    // InAppProduct
    ReceiptAlreadyUsed = 4401,
    SendInAppProductException = 4402,

    // Enhance
    PlayerItemNotExist = 4501,
    NotEnchantableItem = 4502,
    OverMaxEnhanceCount = 4503,
    EnhanceItemException = 4504,

    // Player
    CreatePlayerFailException = 4601,
    FindPlayerIdByAccountIdException = 4602,
    PlayerIdNotExist = 4603,
    DeletePlayerAsyncException = 4604,

    // GameService - Dunjeon
    GetHighestClearedStageException = 4701,
    GetAllStagesAsyncException = 4702,
    InaccessibleStage = 4703,
    EnterStageAsyncException = 4704,
    SaveStageRewardToPlayerException = 4705,
    UpdateHighestClearedStageAsyncException = 4706,   

    // MemoryService - Dunjeon
    ItemFarmingException = 4801,
    KillNpcException = 4802,
    GetFarmedItemsException = 4803,
    AuthUserNotExist = 4804,
    EnterStageFail = 4805,
    MemoryEnterStageAsyncException = 4806,
    UserStateIsNotPlaying = 4807,
    IsPlayingException = 4808,
    OverTotalItemCount = 4809,
    ItemNotExist = 4810,
    OverTotalNpcCount = 4811,
    NpcNotExist = 4812,
    StageNpcExist = 4813,
    CompleteStageException = 4814,
    SetUserStateFail = 4815,
    DeleteNpcRedisFail = 4816,
    DeleteFarmingRedisFail = 4817,
    StopStageException = 4818,

    // MemoryCacheService
    InValidStageId = 4901,

    // MasterService
    LoadItemListFail = 5001,
    LoadItemAttributeListFail = 5002,
    LoadAttendanceCompensationFail = 5003,
    LoadInAppProductListFail = 5004,
    GetAttendanceCompensationExeption = 5005,
    GetInAppItemsByProductIdExeption = 5006,
    IsMoneyException = 5007,
    IsStackableItemException = 5008,
    GetItemByItemIdException = 5009,
    LoadStageItemListException = 5010,
    LoadStageAttackNpcListException = 5011,
    LoadTotalStageCountException = 5012,
    GetTotalStageCountException = 5013,
    GetStageItemsByStageIdException = 5014,
    GetAttackNpcsByStageIdException = 5015,
    GetAttackNpcExpByNpcIdException = 5016,
    GetAttendanceSizeException = 5017,

    // Version
    ClientVersionFailNotMatch = 6001,
    MasterDataVersionFailNotMatch = 6002,

    // Notice
    GetNoticeFailNotExist = 7001,
    GetNoticeException = 7002
}