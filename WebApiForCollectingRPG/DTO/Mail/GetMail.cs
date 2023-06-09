﻿using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace WebApiForCollectingRPG.DTO.Mail;

public class GetMailReq
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

public class GetMailRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public MailDetail Mail { get; set; } = new();
    [Required] public List<ItemDTO> Items{ get; set; } = new();
}

public class MailDetail
{
    public Int64 MailId { get; set; }
    public String Title { get; set; }
    public String Content { get; set; }
    public bool IsReceived { get; set; }
    public bool IsInAppProduct { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationTime { get; set; }
}