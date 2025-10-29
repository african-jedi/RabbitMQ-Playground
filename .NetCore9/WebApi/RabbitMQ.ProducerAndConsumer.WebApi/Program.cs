using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RabbitMQ.ProducerAndConsumer.WebApi.Extensions;
using RabbitMQ.ProducerAndConsumer.WebApi.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Order API",
        Version = "v1",
        Description = "A sample API for demonstrating RabbitMQ Producer and Consumer in Web API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "African Jedi"
        }
    });

    options.MapType<OrderDTO>(() => new OpenApiSchema
    {
        Example = new OpenApiObject
        {
            ["Id"] = new OpenApiInteger(1),
            ["ProductName"] = new OpenApiString("Example Product"),
            ["Quantity"] = new OpenApiInteger(1)
        }
    });
});
builder.AddApplicationServices();

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
//enable OpenAPI in all environments
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();
