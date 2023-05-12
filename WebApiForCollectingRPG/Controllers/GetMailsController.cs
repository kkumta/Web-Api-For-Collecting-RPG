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
public class GetMails : ControllerBase
{
    readonly IGameDb _gameDb;
    readonly IAccountService _accountService;
    readonly ILogger<GetMails> _logger;

    public GetMails(ILogger<GetMails> logger, IGameDb gameDb, IAccountService accountService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _accountService = accountService;
    }

    /**
     * parameter: page
     * return: mailList
     */
    [HttpPost]
    [Route("getMails")]
    public async Task<GetMailsRes> Post(GetMailsReq request)
    {
        var response = new GetMailsRes();

        // 특정 페이지의 우편 목록을 가져온다.
        (var errorCode, var mails) = await _gameDb.GetMailsByPage(request.Page);
        if (errorCode != ErrorCode.None)
        {
            response.Result = errorCode;
            return response;
        }
        foreach(var mail in mails)
        {
            var mailInfo = new MailListInfo
            {
                MailId = mail.MailId,
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