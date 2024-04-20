using Application.RepositoryInterface;
using Application.ServiceImplementation;
using Application.ServiceInterface;
using Infrastructure.DataContext;
using Infrastructure.RepositoryImplementation;
using Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<DataAppContext>(opt =>
    opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.ConfigureCors();

var app = builder.Build();
app.UseCors(options =>
{
    options.AllowAnyOrigin() 
           .AllowAnyMethod() 
           .AllowAnyHeader(); 
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
