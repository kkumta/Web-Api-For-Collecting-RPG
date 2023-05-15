using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class EnterStageReq
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

public class EnterStageRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;

    [Required] public List<Int64> Items { get; set; } = new List<Int64>();
    [Required] public List<AttackNpcDTO> AttackNpcs { get; set; } = new List<AttackNpcDTO>();
}

public class AttackNpcDTO
{
    public Int32 NpcId { get; set; }
    public Int32 NpcCount { get; set; }
}