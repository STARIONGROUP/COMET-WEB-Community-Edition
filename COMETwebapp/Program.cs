// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Program.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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
    
    using COMETwebapp.Extensions;
    using COMETwebapp.Model;
    using COMETwebapp.Resources;
    using COMETwebapp.Shared;
    using COMETwebapp.Shared.SideBarEntry;
    using COMETwebapp.Shared.TopMenuEntry;
    
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

                await app.Services.InitializeCdp4CometCommonServices();

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
