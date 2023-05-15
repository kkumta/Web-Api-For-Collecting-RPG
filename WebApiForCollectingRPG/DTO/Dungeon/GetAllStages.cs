using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Dungeon;

public class GetAllStagesReq
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

public class GetAllStagesRes
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public List<StageDetail> Stages { get; set; } = new();
}

public class StageDetail
{
    public Int32 StageId { get; }
    public bool IsCleared { get; }

    public StageDetail(Int32 stageId, bool isCleared)
    {
        StageId = stageId;
        IsCleared = isCleared;
    }
}