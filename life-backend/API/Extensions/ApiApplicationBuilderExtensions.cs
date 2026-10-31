using Microsoft.Data.SqlClient;

namespace API.Extensions;

public static class ApiApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LIFE API v1"));

            app.MapGet("/house/database", async (IConfiguration configuration) =>
            {
                await using var connection = new SqlConnection(configuration.GetConnectionString("Default"));
                await connection.OpenAsync();
                return Results.Ok(new { database = connection.Database, status = "connected" });
            });
        }

        app.UseHttpsRedirection();
        app.UseCors(ApiServiceCollectionExtensions.CorsPolicyName);
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
