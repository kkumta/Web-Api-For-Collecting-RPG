using System.ComponentModel.DataAnnotations;
using System;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class KillNpcReq
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
    [Required] public Int32 NpcId { get; set; }
}

public class KillNpcRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public bool AllNpcsKilled { get; set; } = false;
}