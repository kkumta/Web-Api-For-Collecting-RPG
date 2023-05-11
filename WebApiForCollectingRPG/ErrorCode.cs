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

    // GameDb
    GetGameDbConnectionFail = 4001,
    CreateAccountGameFailException = 4002,
    CreateAccountItemFailException = 4003,
    GetAccountGameInfoFailNotExist = 4101,
    GetAccountGameInfoFailException = 4102,
    GetAccountItemListFailNotExist = 4103,
    GetAccountItemListFailException = 4104,

    // Mail
    GetMailsFailNotExistPage = 4201,
    GetMailsFailException = 4202,
    GetMailFailNotExist = 4203,
    GetMailFailException = 4204,
    SendRewardToMailboxException = 4205,
    ReceiveMailItemsFailNotExist = 4206,
    ReceiveMailItemsFailMailNotExist = 4207,
    ReceiveMailItemsException = 4208,
    
    // CheckAttendance
    AttendanceInfoNotExist = 4301,
    AttendanceInfoException = 4302,
    DuplicateAttendance = 4303,

    // InAppProduct
    ReceiptAlreadyUsed = 4401,
    SendInAppProductException = 4402,

    // Enhance
    AccountItemNotExist = 4501,
    NotEnchantableItem = 4502,
    OverMaxEnhanceCount = 4503,
    EnhanceItemException = 4504,

    // Player
    CreatePlayerFailException = 4601,
    FindPlayerIdByAccountIdException = 4602,
    PlayerIdNotExist = 4603,

    // MasterDb
    LoadItemListFail = 5001,
    LoadItemAttributeListFail = 5002,
    LoadAttendanceCompensationFail = 5003,
    LoadInAppProductListFail = 5004,
    GetAttendanceCompensationExeption = 5005,
    GetInAppItemsByProductIdExeption = 5006,
    IsMoneyException = 5007,
    IsStackableItemException = 5008,
    GetItemByItemIdException = 5009,

    // Version
    ClientVersionFailNotMatch = 6001,
    MasterDataVersionFailNotMatch = 6002,

    // Notice
    GetNoticeFailNotExist = 7001,
    GetNoticeException = 7002
}
