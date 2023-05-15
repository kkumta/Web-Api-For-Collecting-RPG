using System.ComponentModel.DataAnnotations;
using System;

namespace WebApiForCollectingRPG.DTO.Attendance;

public class CheckAttendanceReq
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

public class CheckAttendanceRes
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
}