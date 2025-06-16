using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de JWT 

// NO SE UTILIZ� LA AUTENTICACI�N PERO, PARA CUESTIONES DE PRACTICA, SE REALIZ� COMO SE VI� EN CLASE LA CONFIGURACION DE ESTA.

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

// Servicios de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuraci�n de la canalizaci�n de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


// LISTA PARA SIMULAR BD DE PRUEBA
var vehiculos = new List<Vehiculo>
{
    new(1, "Toyota", "XX", "VWY001", 2023, "Gris", 16000, "Carro nuevo"),
    new(2, "Mercedes", "XX", "DDS703", 2020, "Gris", 20000, "Carro nuevo"),
    new(3, "Nissan", "XX", "VML0712", 2025, "Gris", 15000, "Carro nuevo"),
    new(4, "Honda", "XX", "SKJ290", 2021, "Gris", 14000, "Carro nuevo"),
};

// Endpoints de la API de veh�culos
app.MapGet("/api/vehiculo", () => Results.Ok(vehiculos))
    .WithTags("Vehiculo")
    .WithName("GetVehiculos")
    .WithSummary("Lista todos los veh�culos")
    .WithDescription("Devuelve una lista completa de veh�culos.")
    .WithOpenApi();

// GET POR PLACA
app.MapGet("/{placa}", (string placa) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    return vehiculo is not null ? Results.Ok(vehiculo) : Results.NotFound();
})
.WithName("GetVehiculo")
.WithSummary("Obtiene un veh�culo por su placa")
.WithDescription("Esta API devuelve los detalles de un veh�culo espec�fico basado en su placa.");


//POST A�ADE NUEVO VEH�CULO
app.MapPost("/", (Vehiculo vehiculo) =>
{
    if (vehiculos.Any(v => v.Placa == vehiculo.Placa))
    {
        return Results.BadRequest("Ya existe un veh�culo con esa placa.");
    }
    vehiculos.Add(vehiculo);
    return Results.Created($"/api/vehiculo/{vehiculo.Placa}", vehiculo);
})
.WithName("PostVehiculo")
.WithSummary("A�ade un nuevo veh�culo")
.WithDescription("Esta API permite a�adir un nuevo veh�culo a la lista.");


//PUT ACTUALIZA VEH�CULO
app.MapPut("/{placa}", (string placa, Vehiculo vehiculoActualizado) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    if (vehiculo is null)
    {
        return Results.NotFound("Veh�culo no encontrado.");
    }
    vehiculo.Marca = vehiculoActualizado.Marca;
    vehiculo.Modelo = vehiculoActualizado.Modelo;
    vehiculo.Anio = vehiculoActualizado.Anio;
    vehiculo.Color = vehiculoActualizado.Color;
    vehiculo.Precio = vehiculoActualizado.Precio;
    vehiculo.Descripcion = vehiculoActualizado.Descripcion;
    return Results.Ok(vehiculo);
})
.WithName("PutVehiculo")
.WithSummary("Actualiza los detalles de un veh�culo")
.WithDescription("Esta API permite actualizar los detalles de un veh�culo existente.");


// DELETE UN VEH�CULO
app.MapDelete("/{placa}", (string placa) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    if (vehiculo is null)
    {
        return Results.NotFound("Veh�culo no encontrado.");
    }
    vehiculos.Remove(vehiculo);
    return Results.NoContent();
})
.WithName("DeleteVehiculo")
.WithSummary("Elimina un veh�culo por su placa")
.WithDescription("Esta API permite eliminar un veh�culo espec�fico de la lista utilizando su placa.");


app.Run();

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