global using FastLog.Core;
global using FastLog.Core.ES.Model;
global using FastLog.Core.Model;
global using Microsoft.Extensions.Caching.Memory;
using FastLog.Core.Aop;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddServerSideBlazor();
builder.Services.AddFastLogReceive("db.json", new FastLogAop());

var app = builder.Build();

app.UseExceptionHandler(error =>
{
    error.Run(async context =>
    {
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            await context.Response.WriteAsync(contextFeature.Error.Message);
        }
    });
});

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();


public class FastLogAop : IFastLogAop
{
    public void EsAdd(EsAddContext context) { }

    public void EsRemove(EsRemoveContext context) { }

    public void MqException(MqExceptionContext context) { }

    public void MqReceive(MqReceiveContext context) { }
}