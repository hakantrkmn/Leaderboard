namespace Leaderboard.Extensions;


public static class Pipeline
{
    public static WebApplication UsePipeline(this WebApplication app)
    {
        if(app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.MapControllers();
        return app;
    }
}