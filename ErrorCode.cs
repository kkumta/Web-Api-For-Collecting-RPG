﻿using System;

public enum ErrorCode :UInt16
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
    LoginFailPwNotMatch = 2004,
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

    //GameDb
    GetGameDbConnectionFail = 4002
}
