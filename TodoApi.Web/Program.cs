using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Todo API v2");
        
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
    });
}

app.UseGlobalExceptionHandler();
app.UseHttpErrorResponses();
app.UseAuthentication();
app.UseAuthorization();
app.MapApiEndpoints();

app.Run();
