using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace BeeEeeLibs.HttpServerBase
{
    public delegate void RegisterDelegate(IEndpointRouteBuilder builder);

    public static class AspNetVerb
    {
        public static void Start(RegisterDelegate[] endPointRegistrations, IPAddress? bindAddress = null, int port = 4000, Action<KestrelServerOptions>? customKestrelConfiguration = null)
        {
            IWebHostBuilder host = new WebHostBuilder()
              .UseKestrel(ko =>
               {
                   ko.Listen(bindAddress ?? IPAddress.Loopback, port);
                   if (customKestrelConfiguration != null)
                   {
                       customKestrelConfiguration(ko);
                   }
               })
              .ConfigureServices(cs =>
               {
                   cs.AddRouting();
               })
              .Configure((wb, app) =>
               {
                   app.UseRouting();
                   app.UseEndpoints(endPointRouteBuilder =>
                   {
                       // register all the endpoints
                       foreach (RegisterDelegate endPointRegistration in endPointRegistrations)
                       {
                           endPointRegistration(endPointRouteBuilder);
                       }
                   });
               });

            host.Build().Run();
        }
    }
}
