using System;

public enum ErrorCode : UInt16
{
    None = 0,

    // Common
    UnhandleException = 1001,
    RedisFailException = 1002,
    InValidRequestHttpBody = 1003,
    AuthTokenFailWrongAuthToken = 1006,

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
    GetMailsFailNotExistPage = 4201,
    GetMailsFailException = 4202,
    GetMailFailNotExist = 4203,
    GetMailFailException = 4204,

    // MasterDb
    GetItemListFail = 5001,
    GetItemAttributeListFail = 5002,
    GetAttendanceCompensationFail = 5003,
    GetInAppProductListFail = 5004,

    // Version
    ClientVersionFailNotMatch = 6001,
    MasterDataVersionFailNotMatch = 6002,

    // Notice
    GetNoticeFailNotExist = 7001,
    GetNoticeException = 7002
}
