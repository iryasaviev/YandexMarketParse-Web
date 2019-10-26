using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace YMParseWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: Проблема в том, что не проходят url строки со слишком большой длиной.
            // https://metanit.com/sharp/aspnet5/11.1.php
            // https://metanit.com/sharp/aspnet5/11.3.php
            services.AddRouting();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RouteHandler routeHandler = new RouteHandler(Handle);
            RouteBuilder routeBuilder = new RouteBuilder(app, routeHandler);

            routeBuilder.MapRoute("default",
                "{controller}/{action}");

            app.UseRouter(routeBuilder.Build());

            app.UseDeveloperExceptionPage();
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseMvc();
        }

        private async Task Handle(HttpContext context)
        {
            await context.Response.WriteAsync("Example: localhost/api/parse/<UrlPath>");
        }
    }
}
