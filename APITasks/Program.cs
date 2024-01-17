using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => 
options.UseInMemoryDatabase("TaksDB"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/tasks", async (AppDbContext context) => await context.Tasks.ToListAsync());

app.MapGet("/tasks/{id}", async (int id, AppDbContext context) => 
    await context.Tasks.FindAsync(id) is Task task ? Results.Ok(task) : Results.NotFound());

app.MapGet("/tasks/done", async (AppDbContext context) => await context.Tasks.Where(t => t.IsDone).ToListAsync());

app.MapPost("/tasks", async (Task task, AppDbContext context) =>
{
    context.Tasks.Add(task);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapGet("tasks/{id}", async (int id, Task request, AppDbContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if(task is null) return Results.NotFound();
    task.Name = request.Name;
    task.IsDone = request.IsDone;
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tasks/{id}", async(int id, AppDbContext context) =>
{
    if (await context.Tasks.FindAsync(id) is Task task)
    {
        context.Remove(task);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

class Task
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsDone { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Task> Tasks => Set<Task>();
}