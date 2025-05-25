using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;
using TicketingSystem.Core.Services.Observers;
using TicketingSystem.Infrastructure.Data;
using TicketingSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IUserRepository<Customer>, UserRepository<Customer>>();
builder.Services.AddScoped<IUserRepository<Organizer>, UserRepository<Organizer>>();
builder.Services.AddScoped<IUserRepository<Administrator>, UserRepository<Administrator>>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<TicketViewService>();
builder.Services.AddScoped<TicketCancellationService>();
builder.Services.AddSingleton<INotificationSubject, NotificationService>();
builder.Services.AddSingleton<EmailNotificationObserver>();
builder.Services.AddSingleton<SMSNotificationObserver>();
builder.Services.AddSingleton<INotificationObserver>(sp => sp.GetRequiredService<EmailNotificationObserver>());
builder.Services.AddSingleton<INotificationObserver>(sp => sp.GetRequiredService<SMSNotificationObserver>());
builder.Services.AddScoped<ISeatMapService, SeatMapService>();
builder.Services.AddScoped<ITicketValidationService, TicketValidationService>();
builder.Services.AddSingleton<PaymentStrategyFactory>();

// Configure HttpClient for the API
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5071/");
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("UserType", "Admin"));
        
    options.AddPolicy("OrganizerOnly", policy =>
        policy.RequireClaim("UserType", "Organizer"));
        
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireClaim("UserType", "Customer"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Register notification observers
var notificationService = app.Services.GetRequiredService<INotificationSubject>();
var emailObserver = app.Services.GetRequiredService<EmailNotificationObserver>();
var smsObserver = app.Services.GetRequiredService<SMSNotificationObserver>();
notificationService.RegisterObserverAsync(emailObserver).GetAwaiter().GetResult();
notificationService.RegisterObserverAsync(smsObserver).GetAwaiter().GetResult();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use session middleware
app.UseSession();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
