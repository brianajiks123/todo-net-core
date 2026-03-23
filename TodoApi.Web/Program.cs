using Microsoft.AspNetCore.Http.HttpResults;
using TodoApi.Web;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────
// Services
// ────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TodoService>();  // in-memory storage

var app = builder.Build();

// ────────────────────────────────────────
// Middleware pipeline
// ────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();   // off when development mode
// app.UseAuthorization();

app.MapGroup("/api")
   .MapTodoEndpoints();

app.Run();
