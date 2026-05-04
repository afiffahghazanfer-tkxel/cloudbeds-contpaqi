using CloudbedsContPAQiIntegration.Configuration;
using CloudbedsContPAQiIntegration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cloudbeds configuration
builder.Services.Configure<CloudbedsSettings>(
    builder.Configuration.GetSection("CloudbedsSettings"));

// Register Cloudbeds HTTP client and services
builder.Services.AddHttpClient<ICloudbedsReservationService, CloudbedsReservationService>();
// In Program.cs
builder.Services.AddHttpClient<CloudbedsReservationService>(client =>
{
    // 1. Set the Base URL
    client.BaseAddress = new Uri("https://hotels.cloudbeds.com/api/v1.3");

    // 2. Add the Authorization Header (Use your cbat_ key)
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "cbat_neLh9Lc117190ZEbkvUd58RhuLSa6Kky");

    // 3. Optional: Add common headers
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
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
app.Run();
