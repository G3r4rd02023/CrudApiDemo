# README

# DemoApiNet10 — API REST con .NET 10 (Minimal API)

Este README explica **paso a paso** cómo crear, configurar y ejecutar la API REST del proyecto **DemoApiNet9** usando .NET 10 y Entity Framework Core con SQL Server. Incluye todos los fragmentos de código necesarios y comandos para correr las migraciones y probar la API.

---

## Requisitos previos

* .NET 10 SDK instalado. Verifica con:

```bash
dotnet --version
```

* SQL Server (local o remoto) accesible.
* (Opcional) Visual Studio 2022/2026, VS Code o tu editor preferido.
* Entity Framework Core tools (si no las tienes):

```bash
dotnet tool install --global dotnet-ef
```

---

## 1. Crear el proyecto

Desde tu terminal crea el proyecto web API con plantilla `webapi`:

```bash
dotnet new webapi -n DemoApiNet9
cd DemoApiNet9
```

El proyecto generado usa Minimal APIs por defecto; borra o ignora los ejemplos generados (por ejemplo `WeatherForecast`).

---

## 2. Estructura recomendada

Crea estas carpetas:

```
/DemoApiNet9
  /Data
  /Models
  /Migrations  (se generará al crear migraciones)
  appsettings.json
  Program.cs
```

---

## 3. Modelo (Entidad)

Crea el archivo `Models/Producto.cs`:

```csharp
namespace DemoApiNet9.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}
```

---

## 4. DbContext (Entity Framework Core)

Crea `Data/AppDbContext.cs`:

```csharp
using DemoApiNet9.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoApiNet9.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
}
```

---

## 5. Cadena de conexión

Edita `appsettings.json` y agrega tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DemoNet9;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Ajusta `Server`, `Database` y credenciales según tu entorno (por ejemplo `User Id` y `Password` si no usas Trusted Connection).

---

## 6. Registrar servicios en `Program.cs` y configurar Swagger

Reemplaza (o modifica) el contenido de `Program.cs` con lo siguiente (archivo completo de ejemplo):

```csharp
using DemoApiNet9.Data;
using DemoApiNet9.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "API REST .NET 9 funcionando!");

// Endpoints CRUD para Productos
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

app.Run();
```

---

## 7. Agregar paquetes NuGet necesarios

Desde la raíz del proyecto ejecuta:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

Si usarás herramientas de EF Core en tiempo de diseño (migrations), asegúrate que `dotnet-ef` esté instalado globalmente como se indicó en Requisitos.

---

## 8. Crear migraciones y actualizar la base de datos

Genera la migración inicial y aplica la base de datos:

```bash
dotnet ef migrations add Inicial
dotnet ef database update
```

> Si aparece un error sobre `dotnet-ef` o `IDesignTimeDbContextFactory`, revisa que el `Program.cs` compile correctamente y que el paquete `Microsoft.EntityFrameworkCore.Design` esté instalado.

---

## 9. Ejecutar la API

Inicia la API:

```bash
dotnet run
```

Abre el navegador en la URL que muestre la consola (por ejemplo `https://localhost:7010/swagger`) para ver Swagger UI y probar los endpoints.

---

## 10. Probar endpoints (ejemplos con curl)

* Listar productos:

```bash
curl https://localhost:7010/api/productos -k
```

* Obtener producto por id:

```bash
curl https://localhost:7010/api/productos/1 -k
```

* Crear producto (POST):

```bash
curl -X POST https://localhost:7010/api/productos -H "Content-Type: application/json" -d '{"nombre":"Camiseta","precio":19.99,"stock":10}' -k
```

* Actualizar producto (PUT):

```bash
curl -X PUT https://localhost:7010/api/productos/1 -H "Content-Type: application/json" -d '{"id":1,"nombre":"Camiseta XL","precio":21.50,"stock":8}' -k
```

* Eliminar producto (DELETE):

```bash
curl -X DELETE https://localhost:7010/api/productos/1 -k
```

> `-k` omite la verificación de certificado cuando usas HTTPS en entorno local.

---

## 11. Consejos y buenas prácticas

* Valida los modelos (Data Annotations) y retorna `BadRequest` cuando correspondan antes de persistir.
* Usa DTOs para separar las entidades de dominio de lo que expones en la API.
* Añade paginación, filtrado y ordenamiento cuando expongas colecciones grandes.
* Considera usar `IRepository`/`UnitOfWork` o capas de servicio si tu API crecerá en complejidad.
* Para producción configura `ConnectionStrings` con variables de entorno o user-secrets y nunca guardes contraseñas en texto.


---

## 13. Problemas comunes y soluciones rápidas

* **Error de conexión a SQL Server**: revisa `Server`, el puerto y que el servicio de SQL Server esté en ejecución. Si usas Docker para SQL, asegúrate de mapear puertos.
* **`dotnet ef` no encontrado**: instala `dotnet-ef` global o usa `dotnet tool restore` si está definido en `manifest`.
* **Migraciones no detectadas**: compila el proyecto primero (`dotnet build`) y vuelve a ejecutar `dotnet ef migrations add`.

---

## 14. Próximos pasos sugeridos

* Implementar autenticación con JWT.
* Añadir tests unitarios e integración (xUnit, WebApplicationFactory).
* Implementar control de versiones de API (v1, v2).
* Crear clientes que consuman la API (Blazor, Angular, React, MAUI).

---

## 15. Referencias

* Documentación oficial .NET: [https://learn.microsoft.com/dotnet/](https://learn.microsoft.com/dotnet/)
* EF Core: [https://learn.microsoft.com/ef/core/](https://learn.microsoft.com/ef/core/)

---


# CrudApiDemo
