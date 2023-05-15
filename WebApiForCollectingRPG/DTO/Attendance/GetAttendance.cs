using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Attendance;

public class GetAttendanceReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    public String Email { get; set; }
    [Required] public String AuthToken { get; set; }
    [Required] public String ClientVersion { get; set; }
    [Required] public String MasterDataVersion { get; set; }
}
public class GetAttendanceRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public List<AttendanceDetail> Attendance { get; set; } = new();
}

public class AttendanceDetail
{
    public Int16 CompensationId { get;}
    public bool IsReceived { get; }

    public AttendanceDetail(Int16 compensationId, bool isReceived)
    {
        CompensationId = compensationId;
        IsReceived = isReceived;
    }
}