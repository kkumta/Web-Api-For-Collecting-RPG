using Microsoft.AspNetCore.Mvc;
using static LogManager;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Dtos;
using WebApiForCollectingRPG.Services;
using Microsoft.Extensions.Logging;
using System;
using ZLogger;
using WebApiForCollectingRPG.Dtos.Game;

namespace WebApiForCollectingRPG.Controllers
{
    [ApiController]
    [Route("api")]
    public class Login : ControllerBase
    {
        readonly IAccountDb _accountDb;
        readonly IMemoryDb _memoryDb;
        readonly IGameDb _gameDb;
        readonly ILogger<Login> _logger;
        public Login(ILogger<Login> logger, IAccountDb accountDb, IMemoryDb memoryDb, IGameDb gameDb)
        {
            _logger = logger;
            _accountDb = accountDb;
            _memoryDb = memoryDb;
            _gameDb = gameDb;
        }


        /**
         * parameter: LoginRequest
         * return: ErrorCode, AccountGame, AccountItem
         */
        [HttpPost]
        [Route("login")]
        public async Task<LoginRes> Post(LoginReq request)
        {
            var response = new LoginRes();

            // Email, Password 검증
            var (errorCode, accountId) = await _accountDb.VerifyAccount(request.Email, request.Password);
            if (errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformationWithPayload(EventIdDic[EventType.Login], new { Email = request.Email }, $"VerifyAccount Success");

            // 계정 검증에 성공한 경우, Token을 만들어 Redis에 저장 후 반환
            var authToken = Security.CreateAuthToken();
            errorCode = await _memoryDb.RegistUserAsync(request.Email, authToken, accountId);
            if (errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }
            response.AuthToken = authToken;

            // 계정 검증에 성공한 경우, 계정 게임 데이터 반환
            (errorCode, var gameInfo) = await _gameDb.GetAccountGameInfoAsync(accountId);
            if (errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }
            response.GameInfo = gameInfo;

            // 계정 검증에 성공한 경우, 계정 아이템 데이터 반환
            (errorCode, var itemList) = await _gameDb.GetAccountItemListAsync(accountId);
            if (errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }
            foreach (var item in itemList)
            {
                var itemInfo = new AccountItem
                {
                    ItemId = item.ItemId,
                    ItemCount = item.ItemCount,
                    EnhanceCount = item.EnhanceCount
                };

                response.ItemInfoList.Add(itemInfo);
            }

            return response;
        }
    }
}