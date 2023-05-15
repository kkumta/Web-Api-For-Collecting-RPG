using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Account;

public class CreateAccountReq
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
}

public class CreateAccountRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}