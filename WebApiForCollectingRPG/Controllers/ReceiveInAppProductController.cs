using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using WebApiForCollectingRPG.DTO.InAppProduct;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class ReceiveInAppProduct : ControllerBase
{
    readonly IGameService _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<ReceiveInAppProduct> _logger;

    public ReceiveInAppProduct(ILogger<ReceiveInAppProduct> logger, IGameService gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    [HttpPost]
    [Route("receiveInAppProduct")]
    public async Task<ReceiptRes> Post(ReceiptReq request)
    {
        var response = new ReceiptRes();

        var errorCode = await _gameDb.SendInAppProduct(request.ReceiptInfo.ReceiptId, request.ReceiptInfo.ProductId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.ReceiveInAppProduct], $"RegisterReceipt Success");
        return response;
    }
}