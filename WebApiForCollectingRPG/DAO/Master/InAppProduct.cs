using System;

namespace WebApiForCollectingRPG.DAO.Master;

public class InAppProduct
{
    public Int16 ProductId { get; set; }
    public Int64 ItemId { get; set; }
    public String ItemName { get; set; }
    public Int32 ItemCount { get; set; } 
}