using Dapper;

using Microsoft.Data.SqlClient;

using System.Data;

namespace MinimalApi.Endpoints.Consump
{
    public static class IncomeRecordEndpoints
    {
        public static void MapIncomeRecordEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/incomerecord-items", async (IncomeRecordObtain request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 定义存储过程需要的参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@startTime", request.startTime);
                    parameters.Add("@endTime", request.endTime);
                    // 执行存储过程
                    var categoryTypes = await connection.QueryAsync<IncomeRecord>(
                        "usp_IncomeRecord_Get", // 存储过程名称
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

            app.MapPost("/api/incomerecord-add", async (IncomeRecordAdd request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@categoryId", request.categoryId);
                    parameters.Add("@incomeAmount", request.incomeAmount);
                    parameters.Add("@incomeTime", request.incomeTime);
                    parameters.Add("@incomeNote", request.incomeNote);
                    // 设置输出参数
                    parameters.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // 2. 执行存储过程
                    await connection.ExecuteAsync(
                        "usp_IncomeRecord_Add", // 存储过程名称
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

            app.MapDelete("/api/incomerecord-delete/{incomeId}", async (int incomeId, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    var sql = "delete IncomeRecord where incomeId = @incomeId";
                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        incomeId
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
    public record IncomeRecordObtain(DateTime startTime, DateTime endTime);
    public record IncomeRecord(int incomeId, string categoryName, decimal incomeAmount,DateTime incomeTime,DateTime createTime,string incomeNote);
    public record IncomeRecordAdd(int categoryId, decimal incomeAmount, DateTime incomeTime, string incomeNote);

}
