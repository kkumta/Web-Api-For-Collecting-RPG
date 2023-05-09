using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using System;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.Services;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class GetMail : ControllerBase
{
    readonly IGameDb _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<GetMail> _logger;

    public GetMail(ILogger<GetMail> logger, IGameDb gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    /**
     * parameter: mailId
     * return: mail
     */
    [HttpPost]
    [Route("mails/{mailId}")]
    public async Task<GetMailRes> Post(GetMailReq request, Int64 mailId)
    {
        var response = new GetMailRes();

        // request의 Email을 가지고 해당하는 AccountId를 찾는다.
        var (errorCode, accountId) = await _accountDb.FindAccountIdByEmail(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        // 해당하는 우편을 가져온다.
        (errorCode, response.Mail, var items) = await _gameDb.GetMailByMailId(accountId, mailId);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach (var item in items)
        {
            var itemInfo = new MailItemInfo
            {
                ItemId = item.ItemId,
                ItemCount = item.ItemCount,
            };

            response.Items.Add(itemInfo);
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.GetMail], $"GetMail Success");
        return response;
    }
}