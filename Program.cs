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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(option => 
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

        ValidateIssuer = false,
        ValidateAudience = false,
    };  
});



builder.Services.AddScoped<IAdministratorServices, AdministratorServices>();
builder.Services.AddScoped<IVehiclesServices, VehiclesServices>();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o Token JWT abaixo:"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
        

        
    });
}

);

builder.Services.AddDbContext<DbContexto>(Options => {
    Options.UseMySql(
        builder.Configuration.GetConnectionString("MySQLConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQLConnection"))
    );
}  );

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administrators

string GenerateTokenJwt(Administrators administrators)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials =  new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims =  new List<Claim>()
    {
        new Claim("Email", administrators.Email),
        new Claim("Perfil", administrators.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorServices administratorServices) => {
    var adm = administratorServices.Login(loginDTO); 
    if(adm != null)
    {
        string token = GenerateTokenJwt(adm);

        return Results.Ok(new LoggedAdmin
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token   
        });

    }

        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administrators");


app.MapGet("/administators/", ([FromQuery] int? pagina, IAdministratorServices administratorServices) => {
    var admins = new List<AdministratorsModelView>();

    var administadores = administratorServices.All(pagina);

    foreach (var adm in administadores)
    {
         admins.Add(new AdministratorsModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil

        });
    }

    return Results.Ok(admins);

}).RequireAuthorization().WithTags("Administrators");



app.MapGet("/administrators/{id}", ([FromRoute]  int id, IAdministratorServices administratorServices) =>
{
    var administrador = administratorServices.SearchId(id); 
    if (administrador == null) return Results.NotFound();


    return Results.Ok(new AdministratorsModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = (administrador.Perfil)

        });
}
).RequireAuthorization().WithTags("Administrators");

app.MapPost("/administrators", ([FromBody] AdministratorsDTO administratorsDTO, IAdministratorServices administratorServices) => {
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
    if ((administratorsDTO.Perfil == null))
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
        Perfil = administratorsDTO.Perfil?.ToString() ?? Perfil.Editor.ToString()
    };

    administratorServices.Include(administrator); 

    return Results.Created($"/administrador/{administrator.Id}", new AdministratorsModelView
        {
            Id = administrator.Id,
            Email = administrator.Email,
            Perfil = (administrator.Perfil)
        });

}).RequireAuthorization().WithTags("Administrators");

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
).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]  int? pagina, IVehiclesServices vehiclesServices) =>
{
    var vehicles = vehiclesServices.All(pagina);

    return Results.Ok(vehicles);
}
).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute]  int id, IVehiclesServices vehiclesServices) =>
{
    var vehicle = vehiclesServices.SearchId(id); 
    if (vehicle == null) return Results.NotFound();


    return Results.Ok(vehicle);
}
).RequireAuthorization().WithTags("Veiculos");

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
).RequireAuthorization().WithTags("Veiculos");


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
).RequireAuthorization().WithTags("Veiculos");

#endregion


#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion

