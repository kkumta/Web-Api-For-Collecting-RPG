using System.IO;
using System.Text.Json;
using System;
using WebApiForCollectingRPG.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;
using WebApiForCollectingRPG.Repository;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IMasterService, MasterService>();
builder.Services.AddTransient<IMemoryCacheService, MemoryCacheService>();
builder.Services.AddTransient<IGameService, GameService>();
builder.Services.AddSingleton<IMemoryService, RedisService>();
builder.Services.AddControllers();

SettingLogger();

var app = builder.Build();

//log setting
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LogManager.SetLoggerFactory(loggerFactory, "Global");

// 앱 실행 시 마스터 데이터 로드
loadMasterData();

app.UseMiddleware<WebApiForCollectingRPG.Middleware.VersionCheck>();
app.UseMiddleware<WebApiForCollectingRPG.Middleware.AuthCheck>();

// Configure the HTTP request pipeline.
app.UseRouting();
#pragma warning disable ASP0014
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
#pragma warning restore ASP0014

var redisDB = app.Services.GetRequiredService<IMemoryService>();
redisDB.Init(configuration.GetSection("DbConfig")["Redis"]);

app.Run(configuration["ServerAddress"]);

void loadMasterData()
{
    var service = app.Services.GetService<IMasterService>();
    service.LoadItemList();
    service.LoadItemAttributeList();
    service.LoadAttendanceCompensation();
    service.LoadInAppProductList();
    service.LoadStageItemList();
    service.LoadStageAttackNpcList();
    service.LoadTotalStageCount();
}

void SettingLogger()
{
    var logging = builder.Logging;
    logging.ClearProviders();

    var fileDir = configuration["logdir"];

    var exists = Directory.Exists(fileDir);

    if (!exists)
    {
        Directory.CreateDirectory(fileDir);
    }

    logging.AddZLoggerRollingFile(
        (dt, x) => $"{fileDir}{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
        x => x.ToLocalTime().Date, 1024,
        options =>
        {
            options.EnableStructuredLogging = true;
            var time = JsonEncodedText.Encode("Timestamp");
            //DateTime.Now는 UTC+0 이고 한국은 UTC+9이므로 9시간을 더한 값을 출력한다.
            var timeValue = JsonEncodedText.Encode(DateTime.Now.AddHours(9).ToString("yyyy/MM/dd HH:mm:ss"));

            options.StructuredLoggingFormatter = (writer, info) =>
            {
                writer.WriteString(time, timeValue);
                info.WriteToJsonWriter(writer);
            };
        }); // 1024KB

    logging.AddZLoggerConsole(options =>
    {
        options.EnableStructuredLogging = true;
        var time = JsonEncodedText.Encode("EventTime");
        var timeValue = JsonEncodedText.Encode(DateTime.Now.AddHours(9).ToString("yyyy/MM/dd HH:mm:ss"));

        options.StructuredLoggingFormatter = (writer, info) =>
        {
            writer.WriteString(time, timeValue);
            info.WriteToJsonWriter(writer);
        };
    });

}