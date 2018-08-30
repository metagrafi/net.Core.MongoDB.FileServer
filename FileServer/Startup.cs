using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileServer.Data;
using FileServer.Repositories;
using FileServer.Services;
using FileServer.WebUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileServer
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
            services.AddMvc().AddJsonOptions(joptions =>
            {
                joptions.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });
            var mongoSettingsSection = Configuration.GetSection("MongoConnection");
            services.Configure<MongoSettings>(mongoSettingsSection);

            services.AddTransient<IMongoStreamer, MongoStreamer>();
            services.AddTransient<IMongoDbContext, FolderDbContext>();
            services.AddTransient<IFilesRepository, FilesRepository>();

            services.AddTransient<MultipartRequestHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
    public class MongoSettings
    {
        public string ConnectionString { get; set; }

        public List<string> Databases { get; set; }
        public List<string> FileCollections { get; set; }
       
    }
}
