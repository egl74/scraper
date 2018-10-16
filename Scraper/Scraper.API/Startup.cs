using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RestSharp;
using RestSharp.Deserializers;
using Scraper.Application.Configuration;
using Scraper.Application.Services;
using Scraper.Contract.Repositories;
using Scraper.Contract.Services;
using Scraper.DAL.Repositories;
using Swashbuckle.AspNetCore.Swagger;

namespace Scraper.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        private readonly string configurationSectionName = "Scraper";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "ShowAPI", Version = "v1" });
            });
            services.AddSingleton(p => Configuration);

            var scraperServiceConfiguration = GetScraperServiceConfiguration();
            services.AddSingleton(scraperServiceConfiguration);
            ConfigureRestClient(services, scraperServiceConfiguration);

            var repositoryConfiguration = GetShowRepositoryConfiguration();
            services.AddSingleton(repositoryConfiguration);
            ConfigureDbClient(services, repositoryConfiguration);
            services.AddTransient<IShowRepository, ShowMongoRepository>();

            services.AddTransient<IDeserializer, JsonDeserializer>();
            services.Configure<ScraperTaskConfiguration>(Configuration.GetSection(configurationSectionName));

            services.AddTransient<IScraperService, ScraperMongoService>();
            services.AddTransient<IShowService, ShowMongoService>();

            services.AddHostedService<ScraperTaskService>();
        }

        public void ConfigureDbClient(IServiceCollection services, ShowRepositoryConfiguration repositoryConfiguration)
        {
            var dbClient = new MongoClient(MongoUrl.Create(repositoryConfiguration.ConnectionString));
            services.AddSingleton<IMongoClient>(dbClient);
        }

        public void ConfigureRestClient(IServiceCollection services, ScraperServiceConfiguration scraperServiceConfiguration)
        {
            var restClient = new RestClient(scraperServiceConfiguration.TVMazeApiAddress);
            services.AddSingleton<IRestClient>(restClient);
        }

        private ScraperServiceConfiguration GetScraperServiceConfiguration()
        {
            var scraperServiceConfiguration = new ScraperServiceConfiguration();
            Configuration.GetSection(configurationSectionName).Bind(scraperServiceConfiguration);
            return scraperServiceConfiguration;
        }

        private ShowRepositoryConfiguration GetShowRepositoryConfiguration()
        {
            var repositoryConfiguration = new ShowRepositoryConfiguration();
            Configuration.GetSection(configurationSectionName).Bind(repositoryConfiguration);
            return repositoryConfiguration;
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShowAPI V1");
            });

            app.UseMvc();
        }
    }
}
