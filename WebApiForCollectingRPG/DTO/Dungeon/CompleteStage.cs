using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class CompleteStageReq
{
    [Required]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    public String Email { get; set; }
    [Required] public String AuthToken { get; set; }
    [Required] public String ClientVersion { get; set; }
    [Required] public String MasterDataVersion { get; set; }
    [Required] public Int32 StageId { get; set; }
}

public class CompleteStageRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
}