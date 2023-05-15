// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="WebAssemblyHostBuilderExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Extensions
{
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Selectors;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions for the <see cref="WebAssemblyHostBuilder"/>
    /// </summary>
    public static class WebAssemblyHostBuilderExtensions
    {
        /// <summary>
        /// Add all required Services and ViewModels for this Library inside the <see cref="WebAssemblyHostBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/></param>
        /// <param name="options">An <see cref="Action{T}"/> to configure this library</param>
        public static void AddCometWebCommon(this WebAssemblyHostBuilder builder, Action<GlobalOptions> options = null)
        {
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            if (options != null)
            {
                builder.Services.Configure(options);
            }

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped(_ => new HttpClient()
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });
            
            builder.RegisterServices();
            builder.RegisterViewModels();
            builder.Services.AddDevExpressBlazor(configure => configure.SizeMode = SizeMode.Medium);
        }

        /// <summary>
        /// Register required Services
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/></param>
        private static void RegisterServices(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddSingleton<AuthenticationStateProvider, CometWebAuthStateProvider>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<IAutoRefreshService, AutoRefreshService>();
            builder.Services.AddSingleton<IVersionService, VersionService>();
            builder.Services.AddSingleton<IRegistrationService, RegistrationService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
        }

        /// <summary>
        /// Register required View Models
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/></param>
        private static void RegisterViewModels(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddTransient<ILoginViewModel, LoginViewModel>();
            builder.Services.AddTransient<IOpenModelViewModel, OpenModelViewModel>();
            builder.Services.AddTransient<IIndexViewModel, IndexViewModel>();
            builder.Services.AddSingleton<IAuthorizedMenuEntryViewModel, AuthorizedMenuEntryViewModel>();
            builder.Services.AddSingleton<ISessionMenuViewModel, SessionMenuViewModel>();
            builder.Services.AddSingleton<IModelMenuViewModel, ModelMenuViewModel>();
            builder.Services.AddTransient<IIterationSelectorViewModel, IterationSelectorViewModel>();
            builder.Services.AddTransient<ISingleIterationApplicationTemplateViewModel, SingleIterationApplicationTemplateViewModel>();
		}
    }
}
