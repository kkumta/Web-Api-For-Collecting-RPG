using System;

namespace WebApiForCollectingRPG.DTO.Mail;

public class MailDTO
{
    public Int64 MailId { get; set; }
    public bool IsReceived { get; set; } = false;
    public bool HasItem { get; set; } = false;
}