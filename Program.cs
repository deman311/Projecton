using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.OpenApi.Models;
using Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// creating a RAM based DB
builder.Services.AddDbContext<ImageContext>(opt => {
    opt.UseInMemoryDatabase("ImageDB");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// This section is a technique used in order to have a fileForm inside Swagger
// source: https://dejanstojanovic.net/aspnet/2021/april/handling-file-upload-in-aspnet-core-5-with-swagger-ui/
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sample.FileUpload.Api", Version = "v1" });
    c.OperationFilter<SwaggerFileOperationFilter>();
});

// Inferense inject the model so that it loads into memory and can be used by the API
builder.Services.AddSingleton<InferenceSession>(
    new InferenceSession("./catdognone.onnx")
);

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
