// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using COMETwebapp.Resources;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.BookEditor;
    using COMETwebapp.ViewModels.Components.Common.OpenTab;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.DomainFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileRevisionHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Publications;
    using COMETwebapp.ViewModels.Components.ModelDashboard;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;
    using COMETwebapp.ViewModels.Components.ParameterEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;
    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.ViewModels.Components.ReferenceData.Categories;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementUnits;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.SiteDirectory;
    using COMETwebapp.ViewModels.Components.SiteDirectory.DomainsOfExpertise;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Organizations;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.UserManagement;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.Viewer;
    using COMETwebapp.ViewModels.Pages;
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
            serviceCollection.AddSingleton<IResourceLoader, ResourceLoader>();
            serviceCollection.AddScoped<ISubscriptionService, SubscriptionService>();
            serviceCollection.AddScoped<IShowHideDeprecatedThingsService, ShowHideDeprecatedThingsService>();
            serviceCollection.AddScoped<ISceneSettings, SceneSettings>();
            serviceCollection.AddScoped<ISelectionMediator, SelectionMediator>();
            serviceCollection.AddScoped<IBabylonInterop, BabylonInterop>();
            serviceCollection.AddScoped<IDomDataService, DomDataService>();
            serviceCollection.AddScoped<IJsUtilitiesService, JsUtilitiesService>();
            serviceCollection.AddHttpClient();
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
            serviceCollection.AddTransient<IElementDefinitionTreeViewModel, ElementDefinitionTreeViewModel>();
            serviceCollection.AddTransient<IModelEditorViewModel, ModelEditorViewModel>();
            serviceCollection.AddTransient<ICopySettingsViewModel, CopySettingsViewModel>();
            serviceCollection.AddTransient<IBookEditorBodyViewModel, BookEditorBodyViewModel>();
            serviceCollection.AddTransient<IMeasurementUnitsTableViewModel, MeasurementUnitsTableViewModel>();
            serviceCollection.AddTransient<IMeasurementScalesTableViewModel, MeasurementScalesTableViewModel>();
            serviceCollection.AddTransient<IDomainsOfExpertiseTableViewModel, DomainsOfExpertiseTableViewModel>();
            serviceCollection.AddTransient<IOrganizationsTableViewModel, OrganizationsTableViewModel>();
            serviceCollection.AddTransient<IEngineeringModelsTableViewModel, EngineeringModelsTableViewModel>();
            serviceCollection.AddTransient<IParticipantsTableViewModel, ParticipantsTableViewModel>();
            serviceCollection.AddTransient<IOrganizationalParticipantsTableViewModel, OrganizationalParticipantsTableViewModel>();
            serviceCollection.AddTransient<IParticipantRolesTableViewModel, ParticipantRolesTableViewModel>();
            serviceCollection.AddTransient<IPersonRolesTableViewModel, PersonRolesTableViewModel>();
            serviceCollection.AddTransient<IEngineeringModelBodyViewModel, EngineeringModelBodyViewModel>();
            serviceCollection.AddTransient<IOptionsTableViewModel, OptionsTableViewModel>();
            serviceCollection.AddTransient<IPublicationsTableViewModel, PublicationsTableViewModel>();
            serviceCollection.AddTransient<ICommonFileStoreTableViewModel, CommonFileStoreTableViewModel>();
            serviceCollection.AddTransient<IDomainFileStoreTableViewModel, DomainFileStoreTableViewModel>();
            serviceCollection.AddTransient<IFolderFileStructureViewModel, FolderFileStructureViewModel>();
            serviceCollection.AddTransient<IFileHandlerViewModel, FileHandlerViewModel>();
            serviceCollection.AddTransient<IFolderHandlerViewModel, FolderHandlerViewModel>();
            serviceCollection.AddTransient<IFileRevisionHandlerViewModel, FileRevisionHandlerViewModel>();
            serviceCollection.AddTransient<IBatchParameterEditorViewModel, BatchParameterEditorViewModel>();
            serviceCollection.AddScoped<ITabsViewModel, TabsViewModel>();
            serviceCollection.AddTransient<IOpenTabViewModel, OpenTabViewModel>();
            serviceCollection.AddTransient<IReferenceDataBodyViewModel, ReferenceDataBodyViewModel>();
            serviceCollection.AddTransient<ISiteDirectoryBodyViewModel, SiteDirectoryBodyViewModel>();
        }
    }
}
