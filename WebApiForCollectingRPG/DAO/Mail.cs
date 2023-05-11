using System;

namespace WebApiForCollectingRPG.DAO;

public class Mail
{
    public Int64 MailId { get; set; }
    public Int64 PlayerId { get; set; }
    public String Title { get; set; }
    public String Content { get; set; }
    public bool IsReceived { get; set; } = false;
    public bool IsInAppProduct { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationTime { get; set; }
    public bool IsDeleted { get; set; } = false;
}