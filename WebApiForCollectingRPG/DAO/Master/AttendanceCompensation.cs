using System;

namespace WebApiForCollectingRPG.DAO.Master;

public class AttendanceCompensation
{
    public Int16 CompensationId { get; set; }
    public Int64 ItemId { get; set; }
    public Int32 ItemCount { get; set; }
}