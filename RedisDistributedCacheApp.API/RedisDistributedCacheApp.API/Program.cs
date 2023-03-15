using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedisDistributedCacheApp.API.Model;
using RedisDistributedCacheApp.API.Repositories;
using RedisDistributedCacheApp.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductRepository>(x =>
{
    var appDbContext = x.GetRequiredService<AppDbContext>();
    var productRepository= new ProductRepository(appDbContext);
    var redisService = x.GetRequiredService<RedisService>();

    return new ProductRepositoryWithCacheDecorator(redisService, productRepository);
});



builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("myDatabase");
});


builder.Services.AddSingleton<RedisService>(x =>
{
    var confing = new ConfigurationOptions
    {
        AbortOnConnectFail = false
    };
    return new RedisService(builder.Configuration["CacheOptions:Url"]);
});

builder.Services.AddSingleton<IDatabase>(x =>
{
    var redisservice = x.GetService<RedisService>();
    return redisservice.GetDb(0);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext= scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

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
