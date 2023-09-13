// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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
    using COMET.Web.Common.Extensions;
    
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Shared.TopMenuEntry;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelDashboard;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor;
    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.UserManagement;
    using COMETwebapp.ViewModels.Components.Viewer;
    using COMETwebapp.ViewModels.Shared.TopMenuEntry;
    
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    
    using System.Diagnostics.CodeAnalysis;
    
    using System.Reflection;

    using COMETwebapp.ViewModels.Components.BookEditor;

    /// <summary>
    /// Point of entry of the application
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Point of entry of the application
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.AddCometWebCommon(options =>
            {
                options.Applications = Applications.ExistingApplications;
                options.AdditionalAssemblies.Add(Assembly.GetAssembly(typeof(Program)));
                options.AdditionalMenuEntries.AddRange(new List<Type>{typeof(ShowHideDeprecatedThings), typeof(AboutMenu)});
            });

            RegisterServices(builder);
            RegisterViewModels(builder);

            var host = builder.Build();
            await host.InitializeServices();
            await host.RunAsync();
        }

        /// <summary>
        /// Register all services required to run the application inside the <see cref="WebAssemblyHostBuilder" />
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder" /></param>
        public static void RegisterServices(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
            builder.Services.AddSingleton<IShowHideDeprecatedThingsService, ShowHideDeprecatedThingsService>();
            builder.Services.AddSingleton<ISceneSettings, SceneSettings>();
            builder.Services.AddSingleton<ISelectionMediator, SelectionMediator>();
            builder.Services.AddSingleton<IDraggableElementService, DraggableElementService>();
            builder.Services.AddSingleton<IBabylonInterop, BabylonInterop>();
            builder.Services.AddAntDesign();
        }

        /// <summary>
        /// Register all view models required to run the application inside the <see cref="WebAssemblyHostBuilder" />
        /// </summary>
        /// <param name="builder">The <see cref="WebAssemblyHostBuilder" /></param>
        public static void RegisterViewModels(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddSingleton<IShowHideDeprecatedThingsViewModel, ShowHideDeprecatedThingsViewModel>();
            builder.Services.AddTransient<IActualFiniteStateSelectorViewModel, ActualFiniteStateSelectorViewModel>();
            builder.Services.AddTransient<IParameterTableViewModel, ParameterTableViewModel>();
            builder.Services.AddTransient<IParameterDashboardViewModel, ParameterDashboardViewModel>();
            builder.Services.AddTransient<IModelDashboardBodyViewModel, ModelDashboardBodyViewModel>();
            builder.Services.AddTransient<ISubscriptionDashboardBodyViewModel, SubscriptionDashboardBodyViewModel>();
            builder.Services.AddTransient<ISubscribedTableViewModel, SubscribedTableViewModel>();
            builder.Services.AddTransient<IParameterEditorBodyViewModel, ParameterEditorBodyViewModel>();
            builder.Services.AddTransient<IViewerBodyViewModel, ViewerBodyViewModel>();
            builder.Services.AddTransient<IElementDefinitionDetailsViewModel, ElementDefinitionDetailsViewModel>();
            builder.Services.AddTransient<IParameterTypeTableViewModel, ParameterTypeTableViewModel>();
            builder.Services.AddTransient<IUserManagementTableViewModel, UserManagementTableViewModel>();
            builder.Services.AddTransient<ICategoriesTableViewModel, CategoriesTableViewModel>();
            builder.Services.AddTransient<ISystemRepresentationBodyViewModel, SystemRepresentationBodyViewModel>();
            builder.Services.AddTransient<IElementDefinitionTableViewModel, ElementDefinitionTableViewModel>();
            builder.Services.AddTransient<IBookEditorBodyViewModel, BookEditorBodyViewModel>();
        }
    }
}
