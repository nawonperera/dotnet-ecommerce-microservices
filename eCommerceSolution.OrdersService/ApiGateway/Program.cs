using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true); //optional=false => I don't wanna make it miss, It should be manditory. You get error or exception if this file is missing.
// reloadOnChange:true => If you make any changes for the ocelot.json file during runtime, the container has to be restarted.

builder.Services.AddOcelot().AddPolly();


var app = builder.Build();
await app.UseOcelot();


app.Run();
