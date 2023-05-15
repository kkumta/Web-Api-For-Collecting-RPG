using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApiForCollectingRPG.DTO.Game;

namespace WebApiForCollectingRPG.DTO.Account;

public class LoginReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    public string Email { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
    [StringLength(30, ErrorMessage = "PASSWORD IS TOO LONG")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required] public string ClientVersion { get; set; }

    [Required] public string MasterDataVersion { get; set; }
}

public class LoginRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public string AuthToken { get; set; } = "";
    [Required] public PlayerGameDTO GameInfo { get; set; } = new();
    [Required] public List<PlayerItemDTO> ItemInfoList { get; set; } = new();
}