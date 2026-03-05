var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
});

var app = builder.Build();

app.MapReverseProxy();

app.Run();
