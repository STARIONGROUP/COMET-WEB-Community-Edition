// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Extensions
{
    using CDP4Common.Validation;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Server.Services.StringTableService;
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
        /// <param name="globalOptions">The optional <see cref="GlobalOptions" /></param>
        public static void RegisterCdp4CometCommonServices(this IServiceCollection serviceProvider, bool isBlazorServer = true, Action<GlobalOptions> globalOptions = null)
        {
            if (isBlazorServer)
            {
                serviceProvider.AddSingleton<IStringTableService, StringTableService>();
            }
            else
            {
                serviceProvider.AddScoped<IStringTableService, WebAssembly.Services.StringTableService.StringTableService>();
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
            serviceProvider.AddAuthorizationCore();
            serviceProvider.AddDevExpressBlazor(configure => configure.SizeMode = SizeMode.Medium);
            serviceProvider.RegisterCommonViewModels();
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
