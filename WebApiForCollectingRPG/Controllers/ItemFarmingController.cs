using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Services;
using static LogManager;
using ZLogger;
using WebApiForCollectingRPG.DTO.Dungeon;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class ItemFarming : ControllerBase
{
    readonly IMemoryService _memoryService;
    readonly ILogger<ItemFarming> _logger;

    public ItemFarming(IMemoryService memoryService, ILogger<ItemFarming> logger)
    {
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpPost]
    [Route("itemFarming")]
    public async Task<ItemFarmingRes> Post(ItemFarmingReq request)
    {
        var response = new ItemFarmingRes();

        var errorCode = await _memoryService.ItemFarmingAsync(request.StageId, request.ItemId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.ItemFarming], $"ItemFarming Success");
        return response;
    }
}