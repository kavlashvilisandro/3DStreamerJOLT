using Streamer.Engine;
using Streamer.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

#region Engine Service
builder.Services.AddGraphicsEngine(builder.Configuration);
#endregion


var app = builder.Build();

app.UseMiddleware<GlobalErrorHandlerMiddleware>();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseWebSockets(new WebSocketOptions(){KeepAliveInterval = TimeSpan.FromSeconds(30)});

app.UseSocketStreaming();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();