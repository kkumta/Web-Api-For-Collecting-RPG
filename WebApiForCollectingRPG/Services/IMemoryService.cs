using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;

namespace WebApiForCollectingRPG.Services
{
    public interface IMemoryService
    {
        public void Init(String address);

        public Task<ErrorCode> RegistUserAsync(String email, String authToken, Int64 accountId, Int64 playerId);

        public Task<ErrorCode> CheckUserAuthAsync(String email, String authToken);

        public Task<(bool, AuthUser)> GetUserAsync(String email);

        public Task<bool> SetUserReqLockAsync(String key);

        public Task<bool> DelUserReqLockAsync(String key);

        public Task<(ErrorCode, Notice)> GetNoticeAsync();
    }
}
