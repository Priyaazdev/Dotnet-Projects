
using Microsoft.EntityFrameworkCore;
using MovieDb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MovieContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/movies", async (MovieContext db) => await db.Movies.ToListAsync());

app.MapGet("/movies/{id}", async (MovieContext db, int id) =>
{
    return await db.Movies.FindAsync(id) switch
    {
        Movie movie => Results.Ok(movie),
        null => Results.NotFound()
    };
});

app.MapPost("/movies", async (MovieContext db, Movie movie) =>
{
    await db.Movies.AddAsync(movie);
    await db.SaveChangesAsync();

    return Results.Created($"/todos/{movie.MovieId}", movie);
});

app.MapPut("/movies/{id}", async (MovieContext db, int id, Movie movie) =>
{
    if (id != movie.MovieId)
    {
        return Results.BadRequest();
    }

    if (!await db.Movies.AnyAsync(x => x.MovieId == id))
    {
        return Results.NotFound();
    }

    db.Update(movie);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapDelete("/movies/{id}", async (MovieContext db, int id) =>
{
    var movie = await db.Movies.FindAsync(id);
    if (movie is null)
    {
        return Results.NotFound();
    }

    db.Movies.Remove(movie);
    await db.SaveChangesAsync();

    return Results.Ok();
});


app.Run();
