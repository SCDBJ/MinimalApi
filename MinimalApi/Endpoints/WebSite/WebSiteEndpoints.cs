using Dapper;

using Microsoft.Data.SqlClient;

using System.Data;

namespace MinimalApi.Endpoints.WebSite
{
    public static class WebSiteEndpoints
    {
        public static void MapWebSiteEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/website-items", async (WebSiteObtain request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 定义存储过程需要的参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@websiteCategory", request.websiteCategory);
                    parameters.Add("@websiteName", request.websiteName);
                    // 执行存储过程
                    var categoryTypes = await connection.QueryAsync<WebSite>(
                        "usp_WebSite_Get", // 存储过程名称
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

            app.MapPost("/api/website-add", async (WebSiteAdd request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@websiteName", request.websiteName);
                    parameters.Add("@websiteHome", request.websiteHome);
                    parameters.Add("@websiteDetail", request.websiteDetail);
                    parameters.Add("@websiteCategory", request.websiteCategory);
                    parameters.Add("@contentTitle", request.contentTitle);
                    parameters.Add("@websiteRemark", request.websiteRemark);
                    parameters.Add("@commonUse", request.commonUse);
                    parameters.Add("@websiteDefault", request.websiteDefault);
                    // 设置输出参数
                    parameters.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // 2. 执行存储过程
                    await connection.ExecuteAsync(
                        "usp_WebSite_Add", // 存储过程名称
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

            app.MapDelete("/api/website-delete/{websiteId}", async (int websiteId, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    var sql = "delete WebSiteCollection where websiteId = @websiteId";
                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        websiteId
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

            app.MapPut("/api/website-update/{websiteId}", async (int websiteId, WebSiteModify dto, IConfiguration config) =>
            {
                await using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                // 编写 SQL，Dapper 允许直接用匿名对象传参
                var sql = "update WebSiteCollection set commonUse = @commonUse where websiteId = @websiteId";

                // 使用 Dapper 的异步执行扩展方法，直接映射参数
                int rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    websiteId,
                    dto.commonUse
                });

                return rowsAffected > 0
                ? Results.Ok(new
                {
                    Success = true,
                    Message = "设置成功"
                })
                : Results.BadRequest(new
                {
                    Success = false,
                    Message = "设置失败"
                });
            });
        }
    }
    public record WebSiteObtain(string websiteCategory, string websiteName);
    public record WebSite(int websiteId, string websiteName, string websiteHome, string websiteDetail, string websiteCategory, string contentTitle, string websiteRemark, string commonUse, DateTime createTime, string websiteDefault);
    public record WebSiteAdd(string websiteName, string websiteHome, string websiteDetail, string websiteCategory, string contentTitle, string websiteRemark, string commonUse, string websiteDefault);
    public record WebSiteModify(string commonUse);
}
