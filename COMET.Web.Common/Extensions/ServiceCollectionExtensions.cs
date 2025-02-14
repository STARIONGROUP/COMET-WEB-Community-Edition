// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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
    using Blazored.SessionStorage;

    using CDP4Common.Validation;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Server.Services.ConfigurationService;
    using COMET.Web.Common.Server.Services.StringTableService;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Publications;
    using COMET.Web.Common.ViewModels.Components.Selectors;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension class for <see cref="IServiceCollection" />
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register required CDP4 Comet common services
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceCollection" /></param>
        /// <param name="isBlazorServer">Value asserting if the application is a Blazor Server application</param>
        /// <param name="globalOptions">The optional <see cref="GlobalOptions"/></param>
        public static void RegisterCdp4CometCommonServices(this IServiceCollection serviceProvider, bool isBlazorServer = true, Action<GlobalOptions> globalOptions = null)
        {
            if (isBlazorServer)
            {
                serviceProvider.AddSingleton<IStringTableService, StringTableService>();
                serviceProvider.AddSingleton<IConfigurationService, ConfigurationService>();
            }
            else
            {
                serviceProvider.AddScoped<IStringTableService, WebAssembly.Services.StringTableService.StringTableService>();
                serviceProvider.AddScoped<IConfigurationService, WebAssembly.Services.ConfigurationService.ConfigurationService>();
            }

            if (globalOptions != null)
            {
                serviceProvider.Configure(globalOptions);
            }

            serviceProvider.AddScoped<ISessionService, SessionService>();
            serviceProvider.AddScoped<AuthenticationStateProvider, CometWebAuthStateProvider>();
            serviceProvider.AddScoped<IAuthenticationService, AuthenticationService>();
            serviceProvider.AddScoped<IAutoRefreshService, AutoRefreshService>();
            serviceProvider.AddSingleton<IVersionService, VersionService>();
            serviceProvider.AddScoped<IRegistrationService, RegistrationService>();
            serviceProvider.AddScoped<INotificationService, NotificationService>();
            serviceProvider.AddScoped<ICDPMessageBus, CDPMessageBus>();
            serviceProvider.AddSingleton<IValidationService, ValidationService>();
            serviceProvider.AddScoped<ICacheService, CacheService>();
            serviceProvider.AddAuthorizationCore();
            serviceProvider.AddDevExpressBlazor(configure => configure.SizeMode = SizeMode.Medium);
            serviceProvider.RegisterCommonViewModels();
            serviceProvider.AddBlazoredSessionStorage();
        }

        /// <summary>
        /// Register required View Models
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceCollection" /></param>
        private static void RegisterCommonViewModels(this IServiceCollection serviceProvider)
        {
            serviceProvider.AddTransient<ILoginViewModel, LoginViewModel>();
            serviceProvider.AddTransient<IOpenModelViewModel, OpenModelViewModel>();
            serviceProvider.AddTransient<IIndexViewModel, IndexViewModel>();
            serviceProvider.AddScoped<IAuthorizedMenuEntryViewModel, AuthorizedMenuEntryViewModel>();
            serviceProvider.AddScoped<ISessionMenuViewModel, SessionMenuViewModel>();
            serviceProvider.AddScoped<IModelMenuViewModel, ModelMenuViewModel>();
            serviceProvider.AddTransient<IIterationSelectorViewModel, IterationSelectorViewModel>();
            serviceProvider.AddTransient<IEngineeringModelSelectorViewModel, EngineeringModelSelectorViewModel>();
            serviceProvider.AddTransient<ISingleIterationApplicationTemplateViewModel, SingleIterationApplicationTemplateViewModel>();
            serviceProvider.AddTransient<ISingleEngineeringModelApplicationTemplateViewModel, SingleEngineeringModelApplicationTemplateViewModel>();
            serviceProvider.AddTransient<IApplicationTemplateViewModel, ApplicationTemplateViewModel>();
            serviceProvider.AddTransient<IPublicationsViewModel, PublicationsViewModel>();
        }
    }
}
