using Microsoft.AspNetCore.Components.Web;
using VentasPOST.ServiceClient.DI;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.WebUIServicesRegister();
await builder.Build().RunAsync();
