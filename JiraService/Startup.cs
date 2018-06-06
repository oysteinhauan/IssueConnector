using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using IssueConnectorLib;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Database;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Services;
using JiraService.Client;
using JiraService.Mapper;
using JiraService.Models;
using JiraService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JiraService
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
			//Configurations
			services.Configure<ConfigSettings>(Configuration.GetSection("ConfigSettings"));
			services.Configure<DatabaseConnectionString>(Configuration.GetSection("ConnectionStrings"));

			//Publisher, subscriber
			services.AddTransient<IResourcePublisher, JiraPublisher>();
			services.AddSingleton<IResourceConsumer, JiraConsumer>();

			//REST-resource
			services.AddSingleton<IResourceCrudService, JiraCrudService>();
			services.AddSingleton<IResourceClient<Jira>, JiraResourceClient>();

			//Objectmappers
			services.AddTransient<IStandardInstanceMapper<IStandardInstance, Issue>, JiraStandardInstanceMapper>();
			services.AddTransient<IStandardCommentMapper<IStandardComment, Comment>, JiraStandardCommentMapper>();

			services.AddTransient<IDatabaseService, DatabaseService>();
			services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

			app.ApplicationServices.GetService<IResourceConsumer>().StartConsumerAsync();

			app.ApplicationServices.GetService<IResourceClient<Jira>>().Connect().Wait();

			app.UseMvc();
        }
    }
}
