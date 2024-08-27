using minimal_api.Infraestrutura.DB;
using minimal_api.Dominio.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Services;
using Microsoft.AspNetCore.Mvc;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Enums;

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
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministratorServices administratorServices) => {
    if (administratorServices.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");


app.MapGet("/administadores", ([FromQuery] int? pagina, IAdministratorServices administratorServices) => {
    var admins = new List<AdministratorsModelView>();

    var administadores = administratorServices.All(pagina);

    foreach (var adm in administadores)
    {
        admins.Add(new AdministratorsModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Profile = (adm.Profile)
        });
    }

}).WithTags("Administradores");



app.MapGet("/administradores/{id}", ([FromRoute]  int id, IAdministratorServices administratorServices) =>
{
    var administrador = administratorServices.SearchId(id); 
    if (administrador == null) return Results.NotFound();


    return Results.Ok(new AdministratorsModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Profile = (administrador.Profile)
        });
}
).WithTags("Administradores");

app.MapPost("/administators", ([FromBody] AdministratorsDTO administratorsDTO, IAdministratorServices administratorServices) => {
    var validation = new ValidationErrors
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(administratorsDTO.Email))
    {
        validation.Messages.Add("Email não pode ser vazio");
        
    }
    if (string.IsNullOrEmpty(administratorsDTO.Senha))
    {
        validation.Messages.Add("Senha não pode ser vazio");
        
    }
    if ((administratorsDTO.Profile == null))
    {
        validation.Messages.Add("Perfil não pode ser vazio");
        
    }

    if(validation.Messages.Count > 0 ) //Verifica se foram incrmentados erros na validação para execução de BadRequest
    {
        return Results.BadRequest(validation);  
    }


    var administrator = new Administrators
    {
        Email = administratorsDTO.Email,
        Senha = administratorsDTO.Senha,
        Profile = administratorsDTO.Profile?.ToString() ?? Profile.Editor.ToString()
    };

    administratorServices.Include(administrator); 

    return Results.Created($"/administrador/{administrator.Id}", new AdministratorsModelView
        {
            Id = administrator.Id,
            Email = administrator.Email,
            Profile = (administrator.Profile)
        });

}).WithTags("Administradores");

#endregion

#region Vehicles

ValidationErrors validateDTO(VehiclesDTO vehiclesDTO)
{
    var validation = new ValidationErrors
    {
        Messages = new List<string>()
    };

    if(string.IsNullOrEmpty(vehiclesDTO.Nome))
    {
        validation.Messages.Add("Valor de \"nome\" não pode ser vazio (null)");
    }

    if(string.IsNullOrEmpty(vehiclesDTO.Marca))
    {
        validation.Messages.Add("Valor de \"marca\" não pode ser vazio (null)");
    }
    
    if(vehiclesDTO.Ano < 1886 || vehiclesDTO.Ano > DateTime.Now.Year) //Valida com base no primeiro carro fabricado e o ano atual do usuario
    {
        validation.Messages.Add("O ano de fabricação deve estar entre 1886 e o ano atual");
    }

    return validation;
}

 app.MapPost("/vehicles", ([FromBody]  VehiclesDTO vehiclesDTO, IVehiclesServices vehiclesServices) =>
{

    var validation = validateDTO(vehiclesDTO); 

    if(validation.Messages.Count > 0 ) //Verifica se foram incrmentados erros na validação para execução de BadRequest
    {
        return Results.BadRequest(validation);  
    }


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
    if (vehicle == null) return Results.NotFound();


    return Results.Ok(vehicle);
}
).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute]  int id, VehiclesDTO vehiclesDTO , IVehiclesServices vehiclesServices) =>
{
    var vehicle = vehiclesServices.SearchId(id); 

    if (vehicle == null) return Results.NotFound();

    var validation = validateDTO(vehiclesDTO); 

    if(validation.Messages.Count > 0) 
    {
        return Results.BadRequest(validation);  
    }
    
        vehicle.Nome =  vehiclesDTO.Nome;
        vehicle.Marca =  vehiclesDTO.Marca;
        vehicle.Ano =  vehiclesDTO.Ano;

        vehiclesServices.Update(vehicle);

        return Results.Ok(vehicle);
    
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

