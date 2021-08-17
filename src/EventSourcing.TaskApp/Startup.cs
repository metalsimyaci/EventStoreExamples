using System;
using Couchbase.Extensions.DependencyInjection;
using EventSourcing.TaskApp.HostedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using EventSourcing.TaskApp.Infrastructure;
using EventSourcing.TaskApp.Infrastructure.Buckets;
using EventStore.ClientAPI;

namespace EventSourcing.TaskApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Event Source Dependency Implementation

            var eventStoreConnection = EventStoreConnection.Create(
                connectionString: Configuration.GetValue<string>("EventStore:ConnectionString"),
                builder: ConnectionSettings.Create().KeepReconnecting(),
                connectionName: Configuration.GetValue<string>("EventStore:ConnectionName"));

            #region Connection Events

            eventStoreConnection.Connected += static (sender, clientConnectionEventArgs) =>
            {
                Console.WriteLine("Baðlantý saðlanmýþtýr.");
                Console.WriteLine($"Connection Name  : {clientConnectionEventArgs.Connection.ConnectionName}");
                Console.WriteLine($"Address Family : {clientConnectionEventArgs.RemoteEndPoint.AddressFamily}");
            };
            eventStoreConnection.Disconnected += (sender, clientConnectionEventArgs) =>
            {
                Console.WriteLine("Baðlantý kesilmiþtir.");
                Console.WriteLine($"Connection Name  : {clientConnectionEventArgs.Connection.ConnectionName}");
                Console.WriteLine($"Address Family : {clientConnectionEventArgs.RemoteEndPoint.AddressFamily}");
            };
            eventStoreConnection.Reconnecting += (sender, clientReconnectingEventArgs) =>
            {
                Console.WriteLine("Baðlantý yeniden deneniyor.");
                Console.WriteLine($"Connection Name  : {clientReconnectingEventArgs.Connection.ConnectionName}");
            };
            eventStoreConnection.ErrorOccurred += (sender, clientErrorEventArgs) =>
            {
                Console.WriteLine("Hata oluþtu!.");
                Console.WriteLine($"Connection Name  : {clientErrorEventArgs.Connection.ConnectionName}");
                Console.WriteLine($"Exception Message : {clientErrorEventArgs.Exception.Message}");
            };

            #endregion

            eventStoreConnection.ConnectAsync().GetAwaiter().GetResult();
            services.AddSingleton(eventStoreConnection);
            services.AddTransient<AggregateRepository>();

            #endregion

            #region Couchbase Dependency Implementation

            services.AddCouchbase((opt) =>
                {
                    opt.ConnectionString = Configuration.GetValue<string>("Couchbase:ConnectionString");
                    opt.UserName = Configuration.GetValue<string>("Couchbase:Username");
                    opt.Password = Configuration.GetValue<string>("Couchbase:Password");

                })
                .AddCouchbaseBucket<ICheckpointBucketProvider>("checkpoints")
                .AddCouchbaseBucket<ITaskBucketProvider>("tasks");

            services.AddTransient<CheckpointRepository>();
            services.AddTransient<TaskRepository>();

            #endregion

            services.AddHostedService<TaskHostedService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventSourcing.TaskApp", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventSourcing.TaskApp v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            hostApplicationLifetime.ApplicationStopped.Register(() =>
            {
                app.ApplicationServices.GetRequiredService<ICouchbaseLifetimeService>().Close();
            });
        }
    }
}
