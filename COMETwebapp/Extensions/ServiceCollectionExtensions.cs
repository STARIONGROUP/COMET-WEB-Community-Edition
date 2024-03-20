// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs" company="RHEA System S.A.">
//     Copyright (c) 2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Extensions
{
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.BookEditor;
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

    /// <summary>
    /// Extension class for the <see cref="IServiceCollection" />
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register all services required to run the application inside the <see cref="IServiceCollection" />
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /></param>
        public static void RegisterServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISubscriptionService, SubscriptionService>();
            serviceCollection.AddScoped<IShowHideDeprecatedThingsService, ShowHideDeprecatedThingsService>();
            serviceCollection.AddScoped<ISceneSettings, SceneSettings>();
            serviceCollection.AddScoped<ISelectionMediator, SelectionMediator>();
            serviceCollection.AddScoped<IDraggableElementService, DraggableElementService>();
            serviceCollection.AddScoped<IBabylonInterop, BabylonInterop>();
            serviceCollection.AddScoped<IDomDataService, DomDataService>();
            serviceCollection.AddHttpClient();
            serviceCollection.AddAntDesign();
        }

        /// <summary>
        /// Register all view models required to run the application inside the <see cref="IServiceCollection" />
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /></param>
        public static void RegisterViewModels(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IShowHideDeprecatedThingsViewModel, ShowHideDeprecatedThingsViewModel>();
            serviceCollection.AddTransient<IParameterTableViewModel, ParameterTableViewModel>();
            serviceCollection.AddTransient<IParameterDashboardViewModel, ParameterDashboardViewModel>();
            serviceCollection.AddTransient<IModelDashboardBodyViewModel, ModelDashboardBodyViewModel>();
            serviceCollection.AddTransient<ISubscriptionDashboardBodyViewModel, SubscriptionDashboardBodyViewModel>();
            serviceCollection.AddTransient<ISubscribedTableViewModel, SubscribedTableViewModel>();
            serviceCollection.AddTransient<IParameterEditorBodyViewModel, ParameterEditorBodyViewModel>();
            serviceCollection.AddTransient<IViewerBodyViewModel, ViewerBodyViewModel>();
            serviceCollection.AddTransient<IElementDefinitionDetailsViewModel, ElementDefinitionDetailsViewModel>();
            serviceCollection.AddTransient<IParameterTypeTableViewModel, ParameterTypeTableViewModel>();
            serviceCollection.AddTransient<IUserManagementTableViewModel, UserManagementTableViewModel>();
            serviceCollection.AddTransient<ICategoriesTableViewModel, CategoriesTableViewModel>();
            serviceCollection.AddTransient<ISystemRepresentationBodyViewModel, SystemRepresentationBodyViewModel>();
            serviceCollection.AddTransient<IElementDefinitionTableViewModel, ElementDefinitionTableViewModel>();
            serviceCollection.AddTransient<IBookEditorBodyViewModel, BookEditorBodyViewModel>();
            serviceCollection.AddTransient<IMeasurementUnitsTableViewModel, MeasurementUnitsTableViewModel>();
            serviceCollection.AddTransient<IMeasurementScalesTableViewModel, MeasurementScalesTableViewModel>();
        }
    }
}
