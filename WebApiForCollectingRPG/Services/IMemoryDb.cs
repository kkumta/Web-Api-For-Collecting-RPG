using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.ModelDB;

namespace WebApiForCollectingRPG.Services
{
    public interface IMemoryDb
    {
        public void Init(String address);

        public Task<ErrorCode> RegistUserAsync(String email, String authToken, Int64 accountId);

        public Task<ErrorCode> CheckUserAuthAsync(String email, String authToken);

        public Task<(bool, AuthUser)> GetUserAsync(String email);

        public Task<bool> SetUserStateAsync(AuthUser user, UserState userState);

        public Task<bool> SetUserReqLockAsync(String key);

        public Task<bool> DelUserReqLockAsync(String key);

        public Task<(ErrorCode, Notice)> GetNoticeAsync();
    }
}
