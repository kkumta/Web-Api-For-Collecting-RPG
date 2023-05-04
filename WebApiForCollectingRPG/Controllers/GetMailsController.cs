using Microsoft.AspNetCore.Mvc;
using static LogManager;
using Microsoft.Extensions.Logging;
using ZLogger;
using WebApiForCollectingRPG.Services;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Mail;
using System;

namespace WebApiForCollectingRPG.Controllers;

[ApiController]
[Route("api")]
public class GetMail : ControllerBase
{
    readonly IGameDb _gameDb;
    readonly IAccountDb _accountDb;
    readonly ILogger<GetMail> _logger;

    public GetMail(ILogger<GetMail> logger, IGameDb gameDb, IAccountDb accountDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountDb = accountDb;
    }

    /**
     * parameter: page
     * return: mailList
     */
    [HttpPost]
    [Route("mails")]
    public async Task<GetMailsRes> Post(GetMailsReq request)
    {
        var response = new GetMailsRes();

        // request의 Email을 가지고 해당하는 AccountId를 찾는다.
        var (errorCode, accountId) = await _accountDb.FindAccountIdByEmail(request.Email);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }

        // 특정 페이지의 우편 목록을 가져온다.
        (errorCode, var mails) = await _gameDb.GetMailsByPage(accountId, request.Page);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach(var mail in mails)
        {
            var mailInfo = new MailListInfo
            {
                Title = mail.Title,
                IsReceived = mail.IsReceived,
                ExpirationTime = mail.ExpirationTime
            };

            response.Mails.Add(mailInfo);   
        }

        _logger.ZLogInformationWithPayload(EventIdDic[EventType.GetMails], $"GetMails Success");        
        return response;
    }
}