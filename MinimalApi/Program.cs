using Dapper;

using MinimalApi.Endpoints.Consump;
using MinimalApi.Endpoints.WebSite;
using MinimalApi.Endpoints.StockTrade;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run("http://127.0.0.1:26500");

