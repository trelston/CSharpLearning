using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Api Versioning:- https://www.telerik.com/blogs/your-guide-rest-api-versioning-aspnet-core
//https://github.com/dotnet/aspnet-api-versioning/wiki
builder.Services.AddApiVersioning(cfg =>
{
    cfg.AssumeDefaultVersionWhenUnspecified = true;
    cfg.DefaultApiVersion = new ApiVersion(1, 0);
    cfg.ReportApiVersions = true;//Informs clients the api versions that are supported in the header
    
    //clients can pass the api version via the header value X-Api-Version
    cfg.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");

    //clients can pass the version via the accept header like this:- application/json;v2
    cfg.ApiVersionReader = new MediaTypeApiVersionReader("v");

    //You can combine the various options
    ApiVersionReader.Combine(
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("version"));
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
