using Dapper;

using Microsoft.Data.SqlClient;

using MinimalApi.Endpoints.Common;

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
                    var incomeRecords = await connection.QueryAsync<IncomeRecord>(
                        "usp_IncomeRecord_Get", // 存储过程名称
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Results.Ok(incomeRecords);
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

            app.MapPost("/api/salaryrecord-items", async (SalaryRecordObtain request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 定义存储过程需要的参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@startYear", request.startYear);
                    parameters.Add("@endYear", request.endYear);
                    // 执行存储过程
                    var salaryRecords = await connection.QueryAsync<SalaryRecord>(
                        "usp_SalaryRecord_Get", // 存储过程名称
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Results.Ok(salaryRecords);
                }
                catch (SqlException ex)
                {
                    // 生产环境建议记录日志，不要直接返回 ex.Message
                    return Results.Problem($"数据库错误: {ex.Message}");
                }
            });

            app.MapPost("/api/salaryrecord-add", async (IConfiguration config) =>
            {
                try
                {
                    List<SalaryItem>? salaryList = SalaryDetail.GetSalary();
                    if (salaryList == null|| salaryList!=null&&salaryList.Count==0)
                    {
                        return Results.BadRequest(new
                       {
                           Success = false,
                           Message = "保存失败"
                       });
                    }

                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@datacyear", salaryList?[0].datacyear);
                    parameters.Add("@datacperiod", salaryList?[0].datacperiod);
                    parameters.Add("@dataf_32", salaryList?[0].dataf_32);
                    parameters.Add("@dataf_131", salaryList?[0].dataf_131);
                    parameters.Add("@dataf_134", salaryList?[0].dataf_134);
                    parameters.Add("@dataf_40", salaryList?[0].dataf_40);
                    parameters.Add("@dataf_94", salaryList?[0].dataf_94);
                    parameters.Add("@dataf_95", salaryList?[0].dataf_95);
                    parameters.Add("@dataf_96", salaryList?[0].dataf_96);
                    parameters.Add("@dataf_97", salaryList?[0].dataf_97);
                    parameters.Add("@dataf_63", salaryList?[0].dataf_63);
                    parameters.Add("@dataf_79", salaryList?[0].dataf_79);
                    parameters.Add("@dataf_158", salaryList?[0].dataf_158);
                    parameters.Add("@dataf_159", salaryList?[0].dataf_159);
                    parameters.Add("@dataf_5", salaryList?[0].dataf_5);
                    parameters.Add("@dataf_3", salaryList?[0].dataf_3);
                    parameters.Add("@dataf_157", salaryList?[0].dataf_157);
                    parameters.Add("@dataf_162", salaryList?[0].dataf_162);
                    parameters.Add("@dataf_163", salaryList?[0].dataf_163);
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
    public record SalaryRecordObtain(int startYear, int endYear);
    public record SalaryRecordAdd(int datacyear, int datacperiod, decimal dataf_32, float dataf_131,float dataf_134,decimal dataf_40,decimal dataf_94, decimal dataf_95,decimal dataf_96,string dataf_97,decimal dataf_63,decimal dataf_79,decimal dataf_158, decimal dataf_159,decimal dataf_5,decimal dataf_3,decimal dataf_157,decimal dataf_162,decimal dataf_163);

    public record SalaryRecord(int salaryid,int datacyear, int datacperiod, decimal dataf_32, double dataf_131, double dataf_134, decimal dataf_40, decimal dataf_94, decimal dataf_95, decimal dataf_96, string dataf_97, decimal dataf_63, decimal dataf_79, decimal dataf_158, decimal dataf_159, decimal dataf_5, decimal dataf_3, decimal dataf_157, decimal dataf_162, decimal dataf_163);
}
