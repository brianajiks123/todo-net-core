using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using TodoApi.Web;
using TodoApi.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerWithJwt();
builder.Services.AddValidation();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));

builder.Services.AddTodoServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomProblemDetails();
builder.Services.AddCustomRateLimiting();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Todo API v2");

        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(DocExpansion.List);
        options.EnableDeepLinking();
        options.EnableFilter();

        // Custom styling
        options.InjectStylesheet("/swagger/custom.css");
        options.InjectJavascript("/swagger/custom.js");

        // Dark theme
        options.ConfigObject.AdditionalItems.Add("theme", "dark");
    });
}

app.UseGlobalExceptionHandler();
app.UseHttpErrorResponses();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapApiEndpoints();

app.Run();
