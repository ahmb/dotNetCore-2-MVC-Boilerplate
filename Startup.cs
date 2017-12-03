using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace Core2test
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnvironment;
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _hostingEnvironment = env;
            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // ConfigureServices runs BEFORE Configure
        //This is where all the Dependency Injection happens, and is the default IoC (inversion of control)
        //contrainer for ASP.NET CORE
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            //Extension methods begging with the work "Add" are helper methods which help resolve ILoggerFactory through DI
            services.AddLogging();
            //services.AddApplicationInsightsTelemetry();
            services.AddMiddlewareAnalysis();
// how to roll your own services
// Transient:
// Transient lifetime services are created each time they are requested. This lifetime works best for lightweight, stateless services.
// Scoped:
// Scoped lifetime services are created once per request.
// Singleton:
// Singleton lifetime services are created the first time they are requested (or when ConfigureServices is run if you specify an instance there) and then every subsequent request will use the same instance. 
            // services.AddTransient<IEmailSender, EmailSenderConcreteClass>()

            //Listing services which may or may not be needed to list all the files in the Files action
            //of the home controller

            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetEntryAssembly());
            var compositeProvider = new CompositeFileProvider(physicalProvider, embeddedProvider);

            // // choose one provider to use for the app and register it (or just use the composite provider, which combines the two)
            // //services.AddSingleton<IFileProvider>(physicalProvider);
            // //services.AddSingleton<IFileProvider>(embeddedProvider);
            services.AddSingleton<IFileProvider>(compositeProvider);
            services.AddDirectoryBrowser();

            //Add a memory cache (only for small applications), and then insert a value in
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory )
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //use sessions 
            app.UseSession();

            // Set up custom content types -associating file extension to MIME type
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".htm3"] = "text/html";
            provider.Mappings[".image"] = "image/png";
            // Replace an existing mapping
            provider.Mappings[".rtf"] = "application/x-msdownload";
            // Remove MP4 videos.
            provider.Mappings.Remove(".mp4");

            app.UseStaticFiles(); //this is for the wwwroot

            app.UseStaticFiles(new StaticFileOptions() //this is for serving content in /MyImages
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", "images")),
                RequestPath = new PathString("/MyImages"),
                //the MIME content type provider is below
                ContentTypeProvider = provider
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", "images")),
                RequestPath = new PathString("/MyImages")
            });


            app.UseStatusCodePages();
            app.UseAuthentication();

            //anonymous lambda functiom
            // app.Run(async context =>
            //         {
            //             await context.Response.WriteAsync("Hello, World!");
            //         });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
