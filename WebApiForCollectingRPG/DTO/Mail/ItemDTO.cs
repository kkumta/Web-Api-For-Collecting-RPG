using System;

namespace WebApiForCollectingRPG.DTO.Mail;

public class ItemDTO
{
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }

    public ItemDTO()
    {
    }

    public ItemDTO(Int64 itemId, Int32 itemCount)
    {
        this.ItemId = itemId;
        this.ItemCount = itemCount;
    }
}