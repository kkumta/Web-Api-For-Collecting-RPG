using System;

namespace WebApiForCollectingRPG.DTO.Mail;

public class MailItemDTO
{
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }

    public MailItemDTO()
    {
    }

    public MailItemDTO(Int64 itemId, Int32 itemCount)
    {
        this.ItemId = itemId;
        this.ItemCount = itemCount;
    }
}