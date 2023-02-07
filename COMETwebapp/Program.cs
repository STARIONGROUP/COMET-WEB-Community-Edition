// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Geren�, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp
{
    using CDP4Dal;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Services.VersionService;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;
    using COMETwebapp.ViewModels.Pages.Viewer;
    using COMETwebapp.ViewModels.Components.ModelDashboard;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Shared.Selectors;
    using COMETwebapp.ViewModels.Pages;
    using COMETwebapp.ViewModels.Pages.ParameterEditor;
    using COMETwebapp.ViewModels.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

    /// <summary>
    /// Point of entry of the application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Point of entry of the application
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            RegisterServices(builder);
            RegisterViewModels(builder);

            await builder.Build().RunAsync();
        }

        /// <summary>
        /// Register all services required to run the application inside the <see cref="WebAssemblyHostBuilder" />
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder" /></param>
        public static void RegisterServices(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped(_ => new HttpClient());

            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddSingleton<ISession, Session>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<AuthenticationStateProvider, CometWebAuthStateProvider>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
            builder.Services.AddSingleton<IAutoRefreshService, AutoRefreshService>();
            builder.Services.AddSingleton<IVersionService, VersionService>();
            builder.Services.AddSingleton<ISceneSettings, SceneSettings>();
            builder.Services.AddSingleton<ISelectionMediator, SelectionMediator>();
            builder.Services.AddSingleton<IBabylonInterop, BabylonInterop>();

            builder.Services.AddDevExpressBlazor(configure => configure.SizeMode = SizeMode.Medium);
            builder.Services.AddAntDesign();
        }

        /// <summary>
        /// Register all view models required to run the application inside the <see cref="WebAssemblyHostBuilder" />
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder" /></param>
        public static void RegisterViewModels(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddTransient<ILoginViewModel, LoginViewModel>();
            builder.Services.AddTransient<IOpenModelViewModel, OpenModelViewModel>();
            builder.Services.AddTransient<IIndexViewModel, IndexViewModel>();
            builder.Services.AddSingleton<IAuthorizedMenuEntryViewModel, AuthorizedMenuEntryViewModel>();
            builder.Services.AddSingleton<ISessionMenuViewModel, SessionMenuViewModel>();
            builder.Services.AddSingleton<IModelMenuViewModel, ModelMenuViewModel>();
            builder.Services.AddTransient<IViewerViewModel, ViewerViewModel>();
            builder.Services.AddTransient<IActualFiniteStateSelectorViewModel, ActualFiniteStateSelectorViewModel>();
            builder.Services.AddTransient<IProductTreeViewModel, ProductTreeViewModel>();
            builder.Services.AddTransient<ICanvasViewModel, CanvasViewModel>();
            builder.Services.AddTransient<IPropertiesComponentViewModel, PropertiesComponentViewModel>();
            builder.Services.AddTransient<IParameterEditorViewModel,ParameterEditorViewModel>();
            builder.Services.AddTransient<ISwitchTooltipViewModel, SwitchTooltipViewModel>();
            builder.Services.AddTransient<IIterationSelectorViewModel, IterationSelectorViewModel>();
            builder.Services.AddTransient<ISingleIterationApplicationTemplateViewModel, SingleIterationApplicationTemplateViewModel>();
            builder.Services.AddTransient<IParameterDashboardViewModel, ParameterDashboardViewModel>();
            builder.Services.AddTransient<IModelDashboardBodyViewModel, ModelDashboardBodyViewModel>();
        }
    }
}
