using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using WebApiForCollectingRPG.DTO.InAppProduct;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class RegisterReceipt : ControllerBase
{
    readonly IGameDb _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<RegisterReceipt> _logger;

    public RegisterReceipt(ILogger<RegisterReceipt> logger, IGameDb gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    [HttpPost]
    [Route("receipts")]
    public async Task<ReceiptRes> Post(ReceiptReq request)
    {
        var response = new ReceiptRes();

        var (errorCode, accountId) = await _accountService.FindAccountIdByEmail(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        errorCode = await _gameDb.SendInAppProduct(accountId, request.receiptInfo.ReceiptId, request.receiptInfo.ProductId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.RegisterReceipt], $"RegisterReceipt Success");
        return response;
    }
}