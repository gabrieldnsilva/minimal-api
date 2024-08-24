using minimal_api.Infraestrutura.DB;
using minimal_api.Dominio.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContexto>(Options => {
    Options.UseMySql(
        builder.Configuration.GetConnectionString("MySQLConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQLConnection"))
    );
}  );

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) => {
    if (loginDTO.Email ==  "adm@teste.com" && loginDTO.Senha == "123456")
        return Results.Ok("Login com sucesso!");
    else
        return Results.Unauthorized();
});

app.Run();

