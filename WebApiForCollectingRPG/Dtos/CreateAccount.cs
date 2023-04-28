using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.Dtos;

public class PkCreateAccountReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage ="E-mail is not valid")]
    public String Email { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "PASSWORD CANNOT BE EMPTY")]
    [StringLength(30, ErrorMessage = "PASSWORD IS TOO LONG")]
    [DataType(DataType.Password)]
    public String Password { get; set; }
}

public class PkCreateAccountRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}
