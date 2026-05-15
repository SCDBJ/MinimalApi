using Dapper;

using Microsoft.Data.SqlClient;

using MinimalApi.Endpoints.WebSite;

using System.Data;


namespace MinimalApi.Endpoints.StockTrade
{
    public static class StockTradingEndpoints
    {
        public static void MapStockTradingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/stocktrade-items", async (StockTradingObtain request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 定义存储过程需要的参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@StockCode", request.StockCode);
                    parameters.Add("@StockName", request.StockName);
                    parameters.Add("@TradeStartDate", request.TradeStartDate);
                    parameters.Add("@TradeEndDate", request.TradeEndDate);
                    // 执行存储过程
                    var categoryTypes = await connection.QueryAsync<StockTrading>(
                        "usp_StockTradingRecord_Get", // 存储过程名称
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Results.Ok(categoryTypes);
                }
                catch (SqlException ex)
                {
                    // 生产环境建议记录日志，不要直接返回 ex.Message
                    return Results.Problem($"数据库错误: {ex.Message}");
                }
            });

            app.MapPost("/api/stocktrade-add", async (StockTradingAdd request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@StockCode", request.StockCode);
                    parameters.Add("@StockName", request.StockName);
                    parameters.Add("@TradeDate", request.TradeDate);
                    parameters.Add("@TradePrice", request.TradePrice);
                    parameters.Add("@TradeShares", request.TradeShares);
                    parameters.Add("@ProfitLossAmount", request.ProfitLossAmount);
                    parameters.Add("@TradeType", request.TradeType);
                    // 设置输出参数
                    parameters.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // 2. 执行存储过程
                    await connection.ExecuteAsync(
                        "usp_StockTradingRecord_Add", // 存储过程名称
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // 3. 获取输出参数的值
                    bool isSuccess = parameters.Get<bool>("@IsSuccess");

                    // 4. 根据状态返回结果
                    return isSuccess
                        ? Results.Ok(new
                        {
                            Success = true,
                            Message = "保存成功"
                        })
                        : Results.BadRequest(new
                        {
                            Success = false,
                            Message = "保存失败"
                        });
                }
                catch (SqlException ex)
                {
                    // 生产环境建议记录日志，不要直接返回 ex.Message
                    return Results.Problem($"数据库错误: {ex.Message}");
                }
            });

            app.MapDelete("/api/stocktrade-delete/{StockId}", async (int StockId, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    var sql = "delete StockTradingRecord where StockId = @StockId";
                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        StockId
                    });

                    return rowsAffected > 0
                        ? Results.Ok(new
                        {
                            Success = true,
                            Message = "删除成功"
                        })
                        : Results.BadRequest(new
                        {
                            Success = false,
                            Message = "删除失败"
                        });
                }
                catch (SqlException ex)
                {
                    // 生产环境建议记录日志，不要直接返回 ex.Message
                    return Results.Problem($"数据库错误: {ex.Message}");
                }
            });
        }
    }
    public record StockTradingObtain(string StockCode,string StockName,DateTime TradeStartDate,DateTime TradeEndDate);
    public record StockTrading(int StockId,string StockCode,string StockName,DateTime TradeDate,decimal TradePrice,decimal TradeShares,decimal ProfitLossAmount,string TradeType);
    public record StockTradingAdd(string StockCode, string StockName, DateTime TradeDate, decimal TradePrice, decimal TradeShares, decimal ProfitLossAmount, string TradeType);
}
