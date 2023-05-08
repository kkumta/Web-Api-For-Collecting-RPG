using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiForCollectingRPG.DTO.Enhance
{
    public class EnhanceItemReq
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

    public class EnhanceItemRes
    {
        [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
        [Required] public bool IsSuccess { get; set; } = false;
    }
}