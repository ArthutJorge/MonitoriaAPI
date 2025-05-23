/*
    Instalações (dotnet restore)
    dotnet add package Microsoft.EntityFrameworkCore -> APIs fundamentais para trabalhar com DbContext e migrations
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer -> Provedor para SQLServer
    dotnet add package Microsoft.EntityFrameworkCore.Design -> comandos de linha dotnet ef para migrations e update de database

    dotnet tool install --global dotnet-ef
    dotnet ef migrations add InitialCreate -> Gera uma migration que descreve as mudanças do database
    dotnet ef database update -> aplica as migrations para o database
*/
using Microsoft.EntityFrameworkCore;
using MonitoriaAPI;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

string[] diasDaSemana = {"Domingo", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado"};

app.MapGet("/", () => "Hello World!");

app.MapPost("/monitores", async (Monitor monitor, AppDbContext db) =>
{
    bool existe = await db.Monitores.AnyAsync(m => m.RA == monitor.RA);
    if (existe)
        return Results.Conflict("Já existe um monitor com esse RA");

    db.Monitores.Add(monitor);
    await db.SaveChangesAsync();
    return Results.Created($"/monitores/{monitor.IdMonitor}", monitor);
});

app.MapPost("/horarios", async (Horario horario, AppDbContext db) =>
{
    var monitorExiste = await db.Monitores.AnyAsync(m => m.IdMonitor == horario.IdMonitor);
    if (!monitorExiste)
        return Results.BadRequest("Monitor não encontrado");

    bool existe = await db.Horarios.AnyAsync(h => h.IdMonitor == horario.IdMonitor && h.DiaSemana == horario.DiaSemana);
    if (existe)
        return Results.Conflict("Já existe horário para esse monitor e dia da semana");

    db.Horarios.Add(horario);
    await db.SaveChangesAsync();
    return Results.Created($"horarios/{horario.IdHorario}", horario);
});


app.MapGet("/monitores", async (AppDbContext db) =>
{
    var monitores = await db.Monitores.Include(m => m.Horarios).ToListAsync();
    return Results.Ok(monitores);
});

app.MapGet("/monitores/{apelido}", async (string apelido, AppDbContext db) =>
{
    var monitor = await db.Monitores
        .Include(m => m.Horarios)
        .FirstOrDefaultAsync(m => m.Apelido == apelido);

    if (monitor is null)
        return Results.NotFound($"Monitor com apelido '{apelido}' não encontrado");

    return Results.Ok(monitor);
});

app.MapPut("/monitores/{idMonitor}", async (int idMonitor, Monitor monitorAtualizado, AppDbContext db) =>
{
    var monitor = await db.Monitores.FindAsync(idMonitor);
    if (monitor is null)
        return Results.NotFound($"Monitor com id '{idMonitor}' não encontrado");

    monitor.Nome = monitorAtualizado.Nome;
    monitor.RA = monitorAtualizado.RA;
    monitor.Apelido = monitorAtualizado.Apelido;

    await db.SaveChangesAsync();
    return Results.Ok(monitor);
});


app.MapPut("/monitores/{idMonitor}/horarios/{diaSemana}", async (int idMonitor, int diaSemana, Horario horarioAtualizado, AppDbContext db) =>
{
    if (diaSemana < 0 || diaSemana > 6)
        return Results.BadRequest("Dia de semana inválido");

    var horario = await db.Horarios
        .SingleOrDefaultAsync(h => h.DiaSemana == diaSemana && h.IdMonitor == idMonitor);
    if (horario is null)
        return Results.NotFound($"Horario de monitor com id '{idMonitor}' em {diasDaSemana[diaSemana]} não encontrado");

    horario.HorarioMonitoria = horarioAtualizado.HorarioMonitoria;

    await db.SaveChangesAsync();
    return Results.Ok(horario);
});

app.MapDelete("/monitores/{idMonitor}", async (int idMonitor, AppDbContext db) =>
{
    var monitor = await db.Monitores
                    .Include(m => m.Horarios)
                    .FirstOrDefaultAsync(m => m.IdMonitor == idMonitor);

    if (monitor is null)
            return Results.NotFound($"Monitor com id '{idMonitor}' não encontrado");
    if (monitor.Horarios.Count != 0)
        return Results.Conflict("Não se pode deletar monitor com horários existentes");

    db.Monitores.Remove(monitor);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/horarios/{idHorario}", async (int idHorario, AppDbContext db) =>
{
    var horario = await db.Horarios.FindAsync(idHorario);
    if (horario is null)
        return Results.NotFound($"Horário com id '{idHorario}' não encontrado");

    db.Horarios.Remove(horario);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.Run();
