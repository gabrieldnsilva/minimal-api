using minimal_api.Infraestrutura.DB;
using minimal_api.Dominio.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Services;
using Microsoft.AspNetCore.Mvc;
using minimal_api.Dominio.ModelViews;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministratorServices, AdministratorServices>();
builder.Services.AddScoped<IVehiclesServices, VehiclesServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(Options => {
    Options.UseMySql(
        builder.Configuration.GetConnectionString("MySQLConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQLConnection"))
    );
}  );

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrators
app.MapPost("/administators/login", ([FromBody] LoginDTO loginDTO, IAdministratorServices administratorServices) => {
    if (administratorServices.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");
#endregion

#region Vehicles
app.MapPost("/vehicles", ([FromBody]  VehiclesDTO vehiclesDTO, IVehiclesServices vehiclesServices) =>
{
    var vehicle = new Vehicles
    {
        Nome = vehiclesDTO.Nome,
        Marca = vehiclesDTO.Marca,
        Ano = vehiclesDTO.Ano
    };

    vehiclesServices.Include(vehicle); 

    return Results.Created($"/Veiculo/{vehicle.Id}", vehicle);
}
).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]  int? pagina, IVehiclesServices vehiclesServices) =>
{
    var vehicles = vehiclesServices.All(pagina);

    return Results.Ok(vehicles);
}
).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute]  int id, IVehiclesServices vehiclesServices) =>
{
    var vehicle = vehiclesServices.SearchId(id); 

    if (vehicle == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(vehicle);
}
).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute]  int id, VehiclesDTO vehiclesDTO , IVehiclesServices vehiclesServices) =>
{
    var vehicle = vehiclesServices.SearchId(id); 

    if (vehicle == null)
    {
        return Results.NotFound();
    }
    else
    {
        vehicle.Nome =  vehiclesDTO.Nome;
        vehicle.Marca =  vehiclesDTO.Marca;
        vehicle.Ano =  vehiclesDTO.Ano;

        vehiclesServices.Update(vehicle);

        return Results.Ok(vehicle);

    }
    
}
).WithTags("Veiculos");


app.MapDelete("/veiculos/{id}", ([FromRoute]  int id, IVehiclesServices vehiclesServices) =>
{
    var vehicle = vehiclesServices.SearchId(id); 

    if (vehicle == null)
    {
        return Results.NotFound();
    }
    else
    {
        
        vehiclesServices.Remove(vehicle);

        return Results.NoContent();

    }
    
}
).WithTags("Veiculos");

#endregion


#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion

