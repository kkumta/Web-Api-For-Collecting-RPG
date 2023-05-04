using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Mail;

public class GetMailsReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    public String Email { get; set; }
    [Required] public String AuthToken { get; set; }
    [Required] public String ClientVersion { get; set; }
    [Required] public String MasterDataVersion { get; set; }
    [Required] public Int32 Page { get; set; }

}


public class GetMailsRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public List<MailListInfo> Mails { get; set; } = new();
}

public class MailListInfo
{
    public String Title { get; set; }
    public bool IsReceived { get; set; }
    public DateTime ExpirationTime { get; set; }
}