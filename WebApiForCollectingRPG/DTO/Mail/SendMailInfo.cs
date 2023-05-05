using System;

namespace WebApiForCollectingRPG.DTO.Mail;

public class SendMailInfo
{
    public Int64 AccountId { get; }
    public String Title { get; }
    public String Content { get; }
    public bool IsReceived { get; } = false;
    public bool IsInAppProduct { get; }
    public DateTime ExpirationTime { get; }
    public bool IsDeleted { get; } = false;

    public SendMailInfo()
    {

    }

    public SendMailInfo(Int64 accountId, String title, String content, bool isInAppProduct, DateTime expirationTime)
    {
        this.AccountId = accountId;
        this.Title = title;
        this.Content = content;
        this.IsInAppProduct = isInAppProduct;
        this.ExpirationTime = expirationTime;
    }
}