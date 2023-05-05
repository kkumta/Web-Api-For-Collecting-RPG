using System;

namespace WebApiForCollectingRPG.DTO.Attendance;

public class AttendanceInfo
{
    public Int16 LastCompensationId { get; set; } = 0;
    public DateTime LastAttendanceDate { get; set; }
}