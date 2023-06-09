﻿using Microsoft.AspNetCore.Mvc;
using static LogManager;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using Microsoft.Extensions.Logging;
using System;
using ZLogger;
using WebApiForCollectingRPG.DTO.Player;
using WebApiForCollectingRPG.DTO.Account;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class Login : ControllerBase
{
    readonly IAccountService _accountService;
    readonly IMemoryService _memoryService;
    readonly IGameService _gameService;
    readonly ILogger<Login> _logger;
    public Login(ILogger<Login> logger, IAccountService accountService, IMemoryService memoryService, IGameService gameService)
    {
        _logger = logger;
        _accountService = accountService;
        _memoryService = memoryService;
        _gameService = gameService;
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

        var (errorCode, accountId) = await _accountService.VerifyAccount(request.Email, request.Password);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.Login], new { Email = request.Email }, $"VerifyAccount Success");

        (errorCode, var playerId) = await _gameService.FindPlayerIdByAccountId(accountId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        _logger.ZLogInformationWithPayload(EventIdDic[EventType.Login], $"FindPlayerIdByAccountId Success");

        var authToken = Security.CreateAuthToken();
        errorCode = await _memoryService.RegistUserAsync(request.Email, authToken, accountId, playerId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        response.AuthToken = authToken;



        (errorCode, var gameInfo) = await _gameService.GetPlayerGameInfoAsync(playerId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        response.GameInfo = gameInfo;

        (errorCode, var itemInfoList) = await _gameService.GetPlayerItemInfoListAsync(playerId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach (var item in itemInfoList)
        {
            var itemInfo = new PlayerItemDTO
            {
                PlayerItemId = item.PlayerItemId,
                ItemId = item.ItemId,
                ItemCount = item.ItemCount,
                EnhanceCount = item.EnhanceCount,
                Attack = item.Attack,
                Defence = item.Defence,
                Magic = item.Magic
            };

            response.ItemInfoList.Add(itemInfo);
        }

        return response;
    }
}