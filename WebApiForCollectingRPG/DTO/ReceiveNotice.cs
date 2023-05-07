using System;
using System.ComponentModel.DataAnnotations;
using WebApiForCollectingRPG.DAO;

namespace WebApiForCollectingRPG.Dtos
{
    public class ReceiveNoticeReq
    {
        [Required] public String Email { get; set; }
        [Required] public String AuthToken { get; set; }
        [Required] public String ClientVersion { get; set; }
        [Required] public String MasterDataVersion { get; set; }
    }

    public class ReceiveNoticeRes
    {
        [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
        public Notice Notice { get; set; }
    }
}