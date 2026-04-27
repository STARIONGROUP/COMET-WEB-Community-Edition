// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Program.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using COMET.Web.Common.Extensions;

    using ReactiveUI.Builder;

    using COMETwebapp.Extensions;
    using COMETwebapp.Health;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Configuration;
    using COMETwebapp.Resources;
    using COMETwebapp.Shared;
    using COMETwebapp.Shared.SideBarEntry;
    using COMETwebapp.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Options;

    using Serilog;

    /// <summary>
    /// Point of entry of the application
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Point of entry of the application
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            Console.Title = "CDP4-COMET WEB";

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            RxAppBuilder.CreateReactiveUIBuilder()
                            .WithBlazor().BuildApp();
            
            builder.Services.RegisterCdp4CometCommonServices(true, options =>
            {
                options.Applications = Applications.ExistingApplications;
                options.AdditionalAssemblies.Add(Assembly.GetAssembly(typeof(Program)));
                options.AdditionalMenuEntries.AddRange([typeof(ApplicationsSideBar), typeof(ShowHideDeprecatedThingsSideBar), typeof(ModelSideBar), typeof(AboutSideBar), typeof(SessionSideBar), typeof(SideBarFooter), typeof(NotificationComponent)]);
                options.MainLayoutType = typeof(SidebarLayout);
            });

            builder.Services.RegisterServices();
            builder.Services.RegisterViewModels();
            builder.Services.AddAntDesign();

            builder.Services.Configure<HealthConfig>(builder.Configuration.GetSection("Health"));
            builder.Services.AddSingleton<ICometHasStartedService, CometHasStartedService>();
            builder.Services.AddSingleton<StartupHealthCheck>();
            builder.Services.AddHealthChecks()
                .AddCheck<StartupHealthCheck>("startup", tags: ["startup", "ready"]);

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom
                    .Configuration(hostingContext.Configuration)
                    .WriteTo.Console();
            });

            var app = builder.Build();

            var logger = app.Services.GetService<ILogger<Program>>();

            try
            {
                var resourceLoader = app.Services.GetService<IResourceLoader>();
                
                logger.LogInformation(resourceLoader.QueryLogo());

                logger.LogInformation("################################################################");

                logger.LogInformation("Starting CDP4-COMET WEB v{version}", resourceLoader.QueryVersion());

                app.UseStaticFiles();
                app.UseRouting();
                app.MapBlazorHub();
                app.MapFallbackToPage("/_Host");

                var healthConfig = app.Services.GetRequiredService<IOptions<HealthConfig>>().Value;

                var liveness = app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
                var startup = app.MapHealthChecks("/health/startup", new HealthCheckOptions { Predicate = c => c.Tags.Contains("startup") });
                var ready = app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

                if (healthConfig.AllowedHosts is { Length: > 0 })
                {
                    liveness.RequireHost(healthConfig.AllowedHosts);
                    startup.RequireHost(healthConfig.AllowedHosts);
                    ready.RequireHost(healthConfig.AllowedHosts);
                }

                await app.Services.InitializeCdp4CometCommonServices();

                app.Services.GetRequiredService<ICometHasStartedService>().MarkStarted();

                logger.LogInformation("CDP4-COMET WEB is running and accepting connections");

                await app.RunAsync();

                logger.LogInformation("Terminated CDP4-COMET WEB cleanly");
                return 0;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "An unhandled exception occurred during startup-bootstrapping");
                return -1;
            }
        }
    }
}
