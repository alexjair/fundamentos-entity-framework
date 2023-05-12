using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoef;
using proyectoef.Models;

var builder = WebApplication.CreateBuilder(args);

//Base de datos en memoria
//builder.Services.AddDbContext<TareasContext>(p => p.UseInMemoryDatabase("TareasDB"));
//bd->sqlsevrer
//builder.Services.AddSqlServer<TareasContext>("Data Source=DESKTOP-FGI8UOO;Initial Catalog=TareasDb;user id=sa;password=12345");
//origen de json configuracion
builder.Services.AddSqlServer<TareasContext>(builder.Configuration.GetConnectionString("cnTareas"));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/dbconexion", async ([FromServices] TareasContext dbContext) => 
{
    dbContext.Database.EnsureCreated();
    return Results.Ok("Base de datos en memoria: " + dbContext.Database.IsInMemory());
});

app.MapGet("/api/tareas", async ([FromServices] TareasContext dbContext)=>
{
    //todos
    //return Results.Ok(dbContext.Tareas);
    //filtro
    //return Results.Ok(dbContext.Tareas.Where(p=> p.PrioridadTarea == proyectoef.Models.Prioridad.Baja));
    //Include Categoria
    return Results.Ok(dbContext.Tareas.Include(p=> p.Categoria).Where(p=> p.PrioridadTarea == proyectoef.Models.Prioridad.Baja));
    /*
    [   {"tareaId":"fe2de405-c38e-4c90-ac52-da0540dfb411","categoriaId":"fe2de405-c38e-4c90-ac52-da0540dfb402","titulo":"Terminar de ver pelicula en netflix","descripcion":null,"prioridadTarea":0,"fechaCreacion":"2023-05-11T00:44:50.486319",
            "categoria":{"categoriaId":"fe2de405-c38e-4c90-ac52-da0540dfb402","nombre":"Actividades personales","descripcion":null,"peso":50},
        "resumen":null}
    ]
    */
});

app.MapGet("/api/task/priority/{id}", async ([FromServices] TareasContext dbContext, int id) => {
    var data = dbContext.Tareas.Include(a => a.Categoria).Where(a => (int)a.PrioridadTarea == id);
    return Results.Ok(data);
});
/*
[{"tareaId":"fe2de405-c38e-4c90-ac52-da0540dfb410","categoriaId":"fe2de405-c38e-4c90-ac52-da0540dfb4ef","titulo":"Pago de servicios publicos","descripcion":null,"prioridadTarea":1,"fechaCreacion":"2023-05-11T00:44:50.4863171","categoria":{"categoriaId":"fe2de405-c38e-4c90-ac52-da0540dfb4ef","nombre":"Actividades pendientes","descripcion":null,"peso":20},"resumen":null}]
*/

app.MapPost("/api/tareas", async ([FromServices] TareasContext dbContext, [FromBody] Tarea tarea)=>
{
    tarea.TareaId = Guid.NewGuid();
    tarea.FechaCreacion = DateTime.Now;
    await dbContext.AddAsync(tarea);
    //await dbContext.Tareas.AddAsync(tarea);

    await dbContext.SaveChangesAsync();

    return Results.Ok();   
});

/*
200OK 2.28 s 92 B
*/

//modificar
app.MapPut("/api/tareas/{id}", async ([FromServices] TareasContext dbContext, [FromBody] Tarea tarea,[FromRoute] Guid id)=>
{
    var tareaActual = dbContext.Tareas.Find(id);
    if(tareaActual!=null)
    {
        tareaActual.CategoriaId = tarea.CategoriaId;
        tareaActual.Titulo = tarea.Titulo;
        tareaActual.PrioridadTarea = tarea.PrioridadTarea;
        tareaActual.Descripcion = tarea.Descripcion;
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();   
});

//DELETE
app.MapDelete("/api/tareas/{id}", async ([FromServices] TareasContext dbContext, [FromRoute] Guid id) =>
{
     var tareaActual = dbContext.Tareas.Find(id);

     if(tareaActual!=null)
     {
         dbContext.Remove(tareaActual);
         await dbContext.SaveChangesAsync();

         return Results.Ok();
     }

     return Results.NotFound();
});


app.Run();
