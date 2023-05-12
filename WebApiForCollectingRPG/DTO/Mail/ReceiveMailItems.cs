using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Mail;

public class ReceiveMailItemsReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    public String Email { get; set; }
    [Required] public String AuthToken { get; set; }
    [Required] public String ClientVersion { get; set; }
    [Required] public String MasterDataVersion { get; set; }
    [Required] public Int64 MailId { get; set; }
}

public class ReceiveMailItemsRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}