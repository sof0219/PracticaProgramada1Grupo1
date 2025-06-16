using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de JWT
var key = "EstaEsUnaClaveSecretaParaFirmarElTokenJWT";
var issuer = "https://localhost:5001";
var audience = "https://localhost:5001";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Datos simulados: usuarios
var usuarios = new List<Auth>
{
    new("admin", "1234"),
    new("usuario", "abcd")
};

// Datos simulados: vehículos
var vehiculos = new List<Vehiculo>
{
    new(1, "Toyota", "XX", "VWY001", 2023, "Gris", 16000, "Carro nuevo"),
    new(2, "Mercedes", "XX", "DDS703", 2020, "Gris", 20000, "Carro nuevo"),
    new(3, "Nissan", "XX", "VML0712", 2025, "Gris", 15000, "Carro nuevo"),
    new(4, "Honda", "XX", "SKJ290", 2021, "Gris", 14000, "Carro nuevo"),
};

// Login: genera token JWT
app.MapPost("/login", (Auth datosLogin) =>
{
    var usuario = usuarios.FirstOrDefault(u =>
        u.Usuario == datosLogin.Usuario && u.Contrasenna == datosLogin.Contrasenna);

    if (usuario is null)
    {
        return Results.Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var clave = Encoding.UTF8.GetBytes(key);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, usuario.Usuario) }),
        Expires = DateTime.UtcNow.AddHours(1),
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(clave), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = tokenString });
})
.WithName("Login")
.WithSummary("Inicio de sesión")
.WithDescription("Genera un token JWT si las credenciales son válidas.");


// GET todos los vehículos (abierto)
app.MapGet("/api/vehiculo", () => Results.Ok(vehiculos))
    .WithTags("Vehiculo")
    .WithName("GetVehiculos")
    .WithSummary("Lista todos los vehículos")
    .WithDescription("Devuelve una lista completa de vehículos.")
    .WithOpenApi();

// GET por placa (abierto)
app.MapGet("/{placa}", (string placa) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    return vehiculo is not null ? Results.Ok(vehiculo) : Results.NotFound();
})
.WithName("GetVehiculo")
.WithSummary("Obtiene un vehículo por su placa")
.WithDescription("Devuelve los detalles de un vehículo basado en su placa.");

// POST nuevo vehículo (requiere autenticación)
app.MapPost("/", (Vehiculo vehiculo) =>
{
    if (vehiculos.Any(v => v.Placa == vehiculo.Placa))
    {
        return Results.BadRequest("Ya existe un vehículo con esa placa.");
    }
    vehiculos.Add(vehiculo);
    return Results.Created($"/api/vehiculo/{vehiculo.Placa}", vehiculo);
})
.RequireAuthorization()
.WithName("PostVehiculo")
.WithSummary("Añade un nuevo vehículo")
.WithDescription("Permite añadir un nuevo vehículo a la lista.");

// PUT actualizar vehículo (requiere autenticación)
app.MapPut("/{placa}", (string placa, Vehiculo vehiculoActualizado) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    if (vehiculo is null)
    {
        return Results.NotFound("Vehículo no encontrado.");
    }
    vehiculo.Marca = vehiculoActualizado.Marca;
    vehiculo.Modelo = vehiculoActualizado.Modelo;
    vehiculo.Anio = vehiculoActualizado.Anio;
    vehiculo.Color = vehiculoActualizado.Color;
    vehiculo.Precio = vehiculoActualizado.Precio;
    vehiculo.Descripcion = vehiculoActualizado.Descripcion;
    return Results.Ok(vehiculo);
})
.RequireAuthorization()
.WithName("PutVehiculo")
.WithSummary("Actualiza los detalles de un vehículo")
.WithDescription("Permite actualizar los datos de un vehículo existente.");

// DELETE un vehículo (requiere autenticación)
app.MapDelete("/{placa}", (string placa) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    if (vehiculo is null)
    {
        return Results.NotFound("Vehículo no encontrado.");
    }
    vehiculos.Remove(vehiculo);
    return Results.NoContent();
})
.RequireAuthorization()
.WithName("DeleteVehiculo")
.WithSummary("Elimina un vehículo")
.WithDescription("Permite eliminar un vehículo por su placa.");

app.Run();

// Modelos
internal record Vehiculo
{
    public int Id { get; set; }
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public string Placa { get; set; }
    public int Anio { get; set; }
    public string Color { get; set; }
    public decimal Precio { get; set; }
    public string Descripcion { get; set; }

    public Vehiculo(int id, string marca, string modelo, string placa, int anio, string color, decimal precio, string descripcion)
    {
        Id = id;
        Marca = marca;
        Modelo = modelo;
        Placa = placa;
        Anio = anio;
        Color = color;
        Precio = precio;
        Descripcion = descripcion;
    }
}

public record Auth(string Usuario, string Contrasenna);
