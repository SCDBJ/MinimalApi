using Dapper;

using Microsoft.Data.SqlClient;

using System.Data;

namespace MinimalApi.Endpoints.Consump
{
    public static class ConsumpRecordEndpoints
    {
        public static void MapConsumpRecordEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/consumprecord-items", async (ConsumpRecordObtain request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 定义存储过程需要的参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@startTime", request.startTime);
                    parameters.Add("@endTime", request.endTime);
                    // 执行存储过程
                    var categoryTypes = await connection.QueryAsync<ConsumpRecord>(
                        "usp_ConsumpRecord_Get", // 存储过程名称
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

            app.MapPost("/api/consumprecord-add", async (ConsumpRecordAdd request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@consumpType", request.consumpType);
                    parameters.Add("@consumpAmount", request.consumpAmount);
                    parameters.Add("@consumpTime", request.consumpTime);
                    parameters.Add("@consumpNote", request.consumpNote);
                    // 设置输出参数
                    parameters.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // 2. 执行存储过程
                    await connection.ExecuteAsync(
                        "usp_ConsumpRecord_Add", // 存储过程名称
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

            app.MapDelete("/api/consumprecord-delete/{consumpId}", async (int consumpId, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    var sql = "delete ConsumpRecord where consumpId = @consumpId";
                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        consumpId
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
    public record ConsumpRecordObtain(DateTime startTime, DateTime endTime);
    public record ConsumpRecord(int consumpId,string consumpType,decimal consumpAmount,DateTime consumpTime,DateTime createTime,string consumpNote);
    public record ConsumpRecordAdd(string consumpType, decimal consumpAmount, DateTime consumpTime,string consumpNote);

}
