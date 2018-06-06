using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EasyAccess.Services;
using EasyAccessService.Client;
using EasyAccessService.Database;
using EasyAccessService.EventListeners;
using EasyAccessService.Mapper;
using EasyAccessService.Models;
using EasyAccessService.Services;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Database;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Services;
using MassTransit.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyAccessService
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
			services.Configure<EasyAccessConfig>(Configuration.GetSection("ConfigSettings"));
			services.Configure<DatabaseConnectionString>(Configuration.GetSection("ConnectionStrings"));

			//Pub-sub
			services.AddTransient<IResourcePublisher, EasyAccessPublisher>();
			services.AddSingleton<IResourceConsumer, EasyAccessConsumer>();

			//Rest-service
			services.AddTransient<IResourceCrudService, EasyAccessCrudService>();

			//RestClient
			services.AddSingleton<IResourceClient<HttpClient>, EasyAccessHttpClient>();

			//Database service
			services.AddTransient<IEasyAccessDatabaseService, EasyAccessDatabaseService>();

			//EasyAccess Event listener
			services.AddSingleton<IEventListener, EasyAccessSignalRListener>();

			//ObjectMappers
			services.AddTransient<IEasyAccessCommentMapper, EasyAccessCommentMapper>();
			services.AddTransient<IEasyAccessStandardInstanceMapper, EasyAccessInstanceMapper>();


			services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
			app.ApplicationServices.GetService<IResourceConsumer>().StartConsumerAsync();

			//Connect and wait for client to be ready
			app.ApplicationServices.GetService<IResourceClient<HttpClient>>().Connect().Wait();

			//Start the SignalR listener
			app.ApplicationServices.GetService<IEventListener>().ConnectAsync();

			app.UseMvc();

			//Stop SignalR listener on application shutdown
			lifetime.ApplicationStopping.Register(() => app.ApplicationServices.GetService<IEventListener>().Stop());
		}
    }
}
