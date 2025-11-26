using CrudApiDemo.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "API REST .NET 9 funcionando!");

app.MapGet("/api/productos", async (AppDbContext context) =>
{
    return await context.Productos.ToListAsync();
});

app.MapGet("/api/productos/{id}", async (int id, AppDbContext context) =>
{
    var producto = await context.Productos.FindAsync(id);
    return producto is null ? Results.NotFound() : Results.Ok(producto);
});

app.MapPost("/api/productos", async (Producto p, AppDbContext context) =>
{
    context.Productos.Add(p);
    await context.SaveChangesAsync();
    return Results.Created($"/api/productos/{p.Id}", p);
});

app.MapPut("/api/productos/{id}", async (int id, Producto data, AppDbContext context) =>
{
    var producto = await context.Productos.FindAsync(id);
    if (producto is null) return Results.NotFound();

    producto.Nombre = data.Nombre;
    producto.Precio = data.Precio;
    producto.Stock = data.Stock;

    await context.SaveChangesAsync();
    return Results.Ok(producto);
});

app.MapDelete("/api/productos/{id}", async (int id, AppDbContext context) =>
{
    var producto = await context.Productos.FindAsync(id);
    if (producto is null) return Results.NotFound();

    context.Productos.Remove(producto);
    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
