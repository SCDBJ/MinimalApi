using Dapper;

using MinimalApi.Endpoints.Consump;
using MinimalApi.Endpoints.WebSite;
using MinimalApi.Endpoints.StockTrade;

using System.Reflection;

// 关键代码：将工作目录切换为当前 .exe 所在的目录
Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // 动态匹配任意 Origin，兼容 credentials
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();           // 允许带凭据
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 显式指定监听 0.0.0.0 和端口
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Any, 26500);
});
// 全局配置 System.Text.Json 序列化格式
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // 自定义一个 DateTime 转换器
    options.SerializerOptions.Converters.Add(new MinimalApi.DateTimeConverter("yyyy-MM-dd HH:mm:ss"));
});



// 启用 Windows 服务支持
builder.Host.UseWindowsService();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//【关键修复点】：在此处激活并启用 CORS 中间件！
// 必须放在 app.MapXxxEndpoints 路由映射之前！
app.UseCors("AllowAll");
//app.UseHttpsRedirection();

// 注册路由扩展
app.MapCategoryEndpoints();
app.MapConsumpRecordEndpoints();
app.MapIncomeRecordEndpoints();
app.MapWebSiteEndpoints();
app.MapStockTradingEndpoints();
app.Run();

