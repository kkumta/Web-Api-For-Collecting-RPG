﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using static LogManager;
using ZLogger;
using WebApiForCollectingRPG.DTO.Dungeon;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class KillNpc : ControllerBase
{
    readonly IMemoryService _memoryService;
    readonly ILogger<KillNpc> _logger;

    public KillNpc(IMemoryService memoryService, ILogger<KillNpc> logger)
    {
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpPost]
    [Route("killNpc")]
    public async Task<KillNpcRes> Post(KillNpcReq request)
    {
        var response = new KillNpcRes();

        var errorCode = await _memoryService.IsPlaying(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        errorCode = await _memoryService.KillNpcAsync(request.Email, request.StageId, request.NpcId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.KillNpc], $"KillNpc Success");

        return response;
    }
}