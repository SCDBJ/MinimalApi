using Dapper;

using Microsoft.Data.SqlClient;

using System.Data;

namespace MinimalApi.Endpoints.Consump
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/category-items", async (CategoryObtain request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 定义存储过程需要的参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@categoryType", request.categoryType);
                    // 执行存储过程
                    var categoryTypes = await connection.QueryAsync<CategoryType>(
                        "usp_Category_Get", // 存储过程名称
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

            app.MapPost("/api/category-add", async (CategoryAdd request, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    // 1. 设置 Dapper 参数
                    var parameters = new DynamicParameters();
                    parameters.Add("@categoryName", request.categoryName);
                    parameters.Add("@categoryType", request.categoryType);
                    parameters.Add("@categoryPriority", request.categoryPriority);
                    // 设置输出参数
                    parameters.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // 2. 执行存储过程
                    await connection.ExecuteAsync(
                        "usp_Category_Add", // 存储过程名称
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

            app.MapDelete("/api/category-delete/{categoryid}", async (int categoryid, IConfiguration config) =>
            {
                try
                {
                    using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

                    var sql = "delete Category where categoryId = @categoryid";
                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        categoryid
                    });

                    return rowsAffected>0
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
    // 请求模型
    public record CategoryObtain(string categoryType);
    public record CategoryAdd(string categoryName,string categoryType,int categoryPriority);
    public record CategoryType(int categoryId, string categoryName, string categoryType, DateTime createTime, int priority);
}
