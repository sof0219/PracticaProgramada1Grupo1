using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de JWT 

// NO SE UTILIZÓ LA AUTENTICACIÓN PERO, PARA CUESTIONES DE PRACTICA, SE REALIZÓ COMO SE VIÓ EN CLASE LA CONFIGURACION DE ESTA.

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

// Configuración de la canalización de solicitudes HTTP
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

// Endpoints de la API de vehículos
app.MapGet("/api/vehiculo", () => Results.Ok(vehiculos))
    .WithTags("Vehiculo")
    .WithName("GetVehiculos")
    .WithSummary("Lista todos los vehículos")
    .WithDescription("Devuelve una lista completa de vehículos.")
    .WithOpenApi();

// GET POR PLACA
app.MapGet("/{placa}", (string placa) =>
{
    var vehiculo = vehiculos.FirstOrDefault(v => v.Placa == placa);
    return vehiculo is not null ? Results.Ok(vehiculo) : Results.NotFound();
})
.WithName("GetVehiculo")
.WithSummary("Obtiene un vehículo por su placa")
.WithDescription("Esta API devuelve los detalles de un vehículo específico basado en su placa.");


//POST AÑADE NUEVO VEHÍCULO
app.MapPost("/", (Vehiculo vehiculo) =>
{
    if (vehiculos.Any(v => v.Placa == vehiculo.Placa))
    {
        return Results.BadRequest("Ya existe un vehículo con esa placa.");
    }
    vehiculos.Add(vehiculo);
    return Results.Created($"/api/vehiculo/{vehiculo.Placa}", vehiculo);
})
.WithName("PostVehiculo")
.WithSummary("Añade un nuevo vehículo")
.WithDescription("Esta API permite añadir un nuevo vehículo a la lista.");


//PUT ACTUALIZA VEHÍCULO
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
.WithName("PutVehiculo")
.WithSummary("Actualiza los detalles de un vehículo")
.WithDescription("Esta API permite actualizar los detalles de un vehículo existente.");


// DELETE UN VEHÍCULO
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
.WithName("DeleteVehiculo")
.WithSummary("Elimina un vehículo por su placa")
.WithDescription("Esta API permite eliminar un vehículo específico de la lista utilizando su placa.");


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