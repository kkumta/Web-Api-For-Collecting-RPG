using System;

namespace WebApiForCollectingRPG.DTO.Mail;

public class SendMailDTO
{
    public Int64 AccountId { get; }
    public String Title { get; }
    public String Content { get; }
    public bool IsReceived { get; } = false;
    public bool IsInAppProduct { get; }
    public bool IsRead { get; } = false;
    public bool HasItem { get; }
    public DateTime ExpirationTime { get; }
    public bool IsDeleted { get; } = false;

    public SendMailDTO()
    {

    }

    public SendMailDTO(Int64 accountId, String title, String content, bool isInAppProduct, bool hasItem, DateTime expirationTime)
    {
        this.AccountId = accountId;
        this.Title = title;
        this.Content = content;
        this.IsInAppProduct = isInAppProduct;
        this.HasItem = hasItem;
        this.ExpirationTime = expirationTime;
    }
}