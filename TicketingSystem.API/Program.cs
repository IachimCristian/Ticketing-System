using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;
using TicketingSystem.Core.Services.Observers;
using TicketingSystem.Infrastructure.Data;
using TicketingSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped(typeof(IUserRepository<>), typeof(UserRepository<>));

// Register services
builder.Services.AddSingleton<INotificationSubject, NotificationService>();
builder.Services.AddSingleton<EmailNotificationObserver>();
builder.Services.AddSingleton<SMSNotificationObserver>();
builder.Services.AddSingleton<INotificationObserver>(sp => sp.GetRequiredService<EmailNotificationObserver>());
builder.Services.AddSingleton<INotificationObserver>(sp => sp.GetRequiredService<SMSNotificationObserver>());
builder.Services.AddSingleton<PaymentStrategyFactory>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<TicketPurchaseFacade>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:5071/");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Register notification observers
var notificationService = app.Services.GetRequiredService<INotificationSubject>();
var emailObserver = app.Services.GetRequiredService<EmailNotificationObserver>();
var smsObserver = app.Services.GetRequiredService<SMSNotificationObserver>();
notificationService.RegisterObserverAsync(emailObserver).GetAwaiter().GetResult();
notificationService.RegisterObserverAsync(smsObserver).GetAwaiter().GetResult();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
