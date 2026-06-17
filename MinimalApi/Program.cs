using Dapper;

using MinimalApi.Endpoints.Consump;
using MinimalApi.Endpoints.WebSite;
using MinimalApi.Endpoints.StockTrade;

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();

// 注册路由扩展
app.MapCategoryEndpoints();
app.MapConsumpRecordEndpoints();
app.MapIncomeRecordEndpoints();
app.MapWebSiteEndpoints();
app.MapStockTradingEndpoints();

//app.Run("http://127.0.0.1:26500");
//app.Run("http://0.0.0.0:26500");
app.Run();

