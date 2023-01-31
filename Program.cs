using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using RateLimiterTest.Clients;
using RateLimiterTest.HttpHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var middlewareFixedPolicy = "fixed";

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(15);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


builder.Services.AddHttpClient<IRateLimitHttpClient, RateLimitHttpClient>()
    .AddHttpMessageHandler<ClientSideRateLimitedHandler>();

builder.Services.AddTransient<ClientSideRateLimitedHandler>(x => new ClientSideRateLimitedHandler(
    new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        TokenLimit = 3,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 3,
        ReplenishmentPeriod = TimeSpan.FromSeconds(20),
        TokensPerPeriod = 1,
        AutoReplenishment = true
    })
));

//builder.Services.AddTransient(x => new ClientSideRateLimitedHandler(
//    new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
//    {
//        PermitLimit = 5,
//        QueueLimit = 4,
//        Window = TimeSpan.FromSeconds(30),
//        AutoReplenishment = true
//    })
//));

//builder.Services.AddTransient(x => new ClientSideRateLimitedHandler(
//    new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
//    {
//        PermitLimit = 5,
//        Window = TimeSpan.FromSeconds(45),
//        SegmentsPerWindow = 3,
//        AutoReplenishment = true
//    })
//));

//builder.Services.AddTransient(x => new ClientSideRateLimitedHandler(
//    new ConcurrencyLimiter(new ConcurrencyLimiterOptions
//    {
//        PermitLimit = 10,
//        QueueLimit = 10
//    })
//));

var app = builder.Build();
app.UseRateLimiter();

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
