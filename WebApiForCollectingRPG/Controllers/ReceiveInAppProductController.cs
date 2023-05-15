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
    readonly IGameService _gameService;
    readonly ILogger<ReceiveInAppProduct> _logger;

    public ReceiveInAppProduct(ILogger<ReceiveInAppProduct> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost]
    [Route("receiveInAppProduct")]
    public async Task<ReceiptRes> Post(ReceiptReq request)
    {
        var response = new ReceiptRes();

        var errorCode = await _gameService.SendInAppProduct(request.ReceiptInfo.ReceiptId, request.ReceiptInfo.ProductId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.ReceiveInAppProduct], $"RegisterReceipt Success");
        return response;
    }
}