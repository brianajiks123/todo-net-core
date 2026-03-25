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
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();
app.UseHttpErrorResponses();
app.UseAuthentication();
app.UseAuthorization();
app.MapApiEndpoints();

app.Run();
