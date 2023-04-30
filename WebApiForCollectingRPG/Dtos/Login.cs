﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiForCollectingRPG.Dtos.Game;

namespace WebApiForCollectingRPG.Dtos;

public class LoginReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    public String Email { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
    [StringLength(30, ErrorMessage = "PASSWORD IS TOO LONG")]
    [DataType(DataType.Password)]
    public String Password { get; set; }
}

public class LoginRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public String AuthToken { get; set; } = "";
    [Required] public AccountGame GameInfo { get; set; } = new();
    [Required] public List<AccountItem> ItemInfoList { get; set; } = new();
}