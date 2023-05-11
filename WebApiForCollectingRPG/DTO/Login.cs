using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiForCollectingRPG.DTO.Game;

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

    [Required] public String ClientVersion { get; set; }

    [Required] public String MasterDataVersion { get; set; }
}

public class LoginRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public String AuthToken { get; set; } = "";
    [Required] public PlayerGameInfo GameInfo { get; set; } = new();
    [Required] public List<PlayerItemInfo> ItemInfoList { get; set; } = new();
}