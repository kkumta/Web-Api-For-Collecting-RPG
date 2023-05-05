using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;
using Microsoft.AspNetCore.Http;

namespace WebApiForCollectingRPG.Middleware;

public class SecurityAuthentication
{
    private readonly Services.IMemoryDb _memoryDb;
    private readonly RequestDelegate _next;

    public SecurityAuthentication(RequestDelegate next, Services.IMemoryDb memoryDb)
    {
        _memoryDb = memoryDb;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var formString = context.Request.Path.Value;
        if (string.Compare(formString, "/api/login", StringComparison.OrdinalIgnoreCase) == 0 ||
            string.Compare(formString, "/api/account", StringComparison.OrdinalIgnoreCase) == 0)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(context);

            return;
        }

        // https://devblogs.microsoft.com/dotnet/re-reading-asp-net-core-request-bodies-with-enablebuffering/
        // 다중 읽기 허용 함수 -> 파일 형식으로 임시 변환
        context.Request.EnableBuffering();

        string email;
        string AuthToken;
        string userLockKey = "";
        string clientVersion;
        string masterDataVersion;

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();

            // body String에 어떤 문자열도 없을때
            if (await IsNullBodyDataThenSendError(context, bodyStr))
            {
                return;
            }


            var document = JsonDocument.Parse(bodyStr);

            if (IsInvalidJsonFormatThenSendError(context, document, out email, out AuthToken, out clientVersion, out masterDataVersion))
            {
                return;
            }

            // 클라이언트 버전 확인
            if (await IsInvalidClientVersionThenSendError(context, clientVersion))
            {
                return;
            }

            // 마스터 데이터 버전 확인
            if (await IsInvalidMasterDataVersionThenSendError(context, masterDataVersion))
            {
                return;
            }


            var (isOk, userInfo) = await _memoryDb.GetUserAsync(email);
            if (isOk == false)
            {
                return;
            }

            if (await IsInvalidUserAuthTokenThenSendError(context, userInfo, AuthToken))
            {
                return;
            }

            userLockKey = Services.MemoryDbKeyMaker.MakeUserLockKey(userInfo.Email);
            if (await SetLockAndIsFailThenSendError(context, userLockKey))
            {
                return;
            }

            context.Items[nameof(AuthUser)] = userInfo;
        }

        context.Request.Body.Position = 0;

        // Call the next delegate/middleware in the pipeline
        await _next(context);

        // 트랜잭션 해제(Redis 동기화 해제)
        await _memoryDb.DelUserReqLockAsync(userLockKey);
    }

    private static async Task<bool> IsInvalidClientVersionThenSendError(HttpContext context, string clientVersion)
    {
        string ClientVersion = "1.0.0";
        if (clientVersion.CompareTo(ClientVersion) == 0)
        {
            return false;
        }

        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
        {
            result = ErrorCode.ClientVersionFailNotMatch
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        return true;
    }

    private static async Task<bool> IsInvalidMasterDataVersionThenSendError(HttpContext context, string masterDataVersion)
    {
        string MasterDataVersion = "1.0.0";
        if (masterDataVersion.CompareTo(MasterDataVersion) == 0)
        {
            return false;
        }

        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
        {
            result = ErrorCode.MasterDataVersionFailNotMatch
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        return true;
    }

    private async Task<bool> SetLockAndIsFailThenSendError(HttpContext context, string AuthToken)
    {
        if (await _memoryDb.SetUserReqLockAsync(AuthToken))
        {
            return false;
        }


        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
        {
            result = ErrorCode.AuthTokenFailSetNx
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        return true;
    }

    private static async Task<bool> IsInvalidUserAuthTokenThenSendError(HttpContext context, AuthUser userInfo, string authToken)
    {
        // 요청 body의 AuthToken과 Redis의 AuthToken이 같은가?
        if (string.CompareOrdinal(userInfo.AuthToken, authToken) == 0)
        {
            return false;
        }


        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
        {
            result = ErrorCode.AuthTokenFailWrongAuthToken
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        return true;
    }

    private bool IsInvalidJsonFormatThenSendError(HttpContext context, JsonDocument document, out string email,
        out string authToken, out string clientVersion, out string masterDataVersion)
    {
        try
        {
            email = document.RootElement.GetProperty("Email").GetString();
            authToken = document.RootElement.GetProperty("AuthToken").GetString();
            clientVersion = document.RootElement.GetProperty("ClientVersion").GetString();
            masterDataVersion = document.RootElement.GetProperty("MasterDataVersion").GetString();

            return false;
        }
        catch
        {
            email = ""; authToken = ""; clientVersion = ""; masterDataVersion = "";

            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                result = ErrorCode.AuthTokenFailWrongKeyword
            });

            var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
            context.Response.Body.Write(bytes, 0, bytes.Length);

            return true;
        }
    }

    async Task<bool> IsNullBodyDataThenSendError(HttpContext context, string bodyStr)
    {
        if (string.IsNullOrEmpty(bodyStr) == false)
        {
            return false;
        }

        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
        {
            result = ErrorCode.InValidRequestHttpBody
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        return true;
    }


    public class MiddlewareResponse
    {
        public ErrorCode result { get; set; }
    }
}