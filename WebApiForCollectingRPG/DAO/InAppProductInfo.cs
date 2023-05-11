using System;

namespace WebApiForCollectingRPG.DTO.InAppProduct;

public class Receipt
{
    public Int64 ReceiptId { get; set; }
    public Int64 PlayerId { get; set; }
    public Int16 ProductId { get; set; }
}