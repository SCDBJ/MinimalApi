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

            app.MapPost("/api/salaryrecord-add", async (SalaryRecordAdd request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@datacyear", request.datacyear);
                    parameters.Add("@datacperiod", request.datacperiod);
                    parameters.Add("@dataf_32", request.dataf_32);
                    parameters.Add("@dataf_131", request.dataf_131);
                    parameters.Add("@dataf_134", request.dataf_134);
                    parameters.Add("@dataf_40", request.dataf_40);
                    parameters.Add("@dataf_94", request.dataf_94);
                    parameters.Add("@dataf_95", request.dataf_95);
                    parameters.Add("@dataf_96", request.dataf_96);
                    parameters.Add("@dataf_97", request.dataf_97);
                    parameters.Add("@dataf_63", request.dataf_63);
                    parameters.Add("@dataf_79", request.dataf_79);
                    parameters.Add("@dataf_158", request.dataf_158);
                    parameters.Add("@dataf_159", request.dataf_159);
                    parameters.Add("@dataf_5", request.dataf_5);
                    parameters.Add("@dataf_3", request.dataf_3);
                    parameters.Add("@dataf_157", request.dataf_157);
                    parameters.Add("@dataf_162", request.dataf_162);
                    parameters.Add("@dataf_163", request.dataf_163);
                    // 设置输出参数
                    parameters.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // 2. 执行存储过程
                    await connection.ExecuteAsync(
                        "usp_SalaryRecord_Add", // 存储过程名称
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
        }
    }
    public record IncomeRecordObtain(DateTime startTime, DateTime endTime);
    public record IncomeRecord(int incomeId, string categoryName, decimal incomeAmount,DateTime incomeTime,DateTime createTime,string incomeNote);
    public record IncomeRecordAdd(int categoryId, decimal incomeAmount, DateTime incomeTime, string incomeNote);
    public record SalaryRecordAdd(int datacyear, int datacperiod, decimal dataf_32, float dataf_131,float dataf_134,decimal dataf_40,decimal dataf_94, decimal dataf_95,decimal dataf_96,string dataf_97,decimal dataf_63,decimal dataf_79,decimal dataf_158, decimal dataf_159,decimal dataf_5,decimal dataf_3,decimal dataf_157,decimal dataf_162,decimal dataf_163);


}
