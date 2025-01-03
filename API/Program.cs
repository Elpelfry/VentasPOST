using VentasPOST.ServiceServer.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.APIServicesRegister();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("WebUI");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
