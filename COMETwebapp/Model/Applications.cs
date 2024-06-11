// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Applications.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Model
{
    using COMET.Web.Common.Model;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Components.ModelDashboard;
    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.Components.SubscriptionDashboard;
    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Pages.ModelEditor;
    using COMETwebapp.Utilities;

    /// <summary>
    /// Provides all available application contained into the current web application
    /// </summary>
    public static class Applications
    {
        /// <summary>
        /// A collection of <see cref="Application" />
        /// </summary>
        private static List<Application> applications;

        /// <summary>
        /// List of <see cref="Application" /> with Name, Color, Icon and Description data
        /// </summary>
        public static List<Application> ExistingApplications => applications ?? InitializesApplications();

        /// <summary>
        /// Initializes all <see cref="Application" />
        /// </summary>
        /// <returns>The initialized <see cref="Application" /></returns>
        private static List<Application> InitializesApplications()
        {
            applications =
            [
                new TabbedApplication
                {
                    Name = "Parameter Editor",
                    Color = "#76b8fc",
                    Icon = "spreadsheet",
                    Description = "Table of element usages with their associated parameters.",
                    Url = WebAppConstantValues.ParameterEditorPage,
                    ComponentType = typeof(ParameterEditorBody)
                },

                new TabbedApplication
                {
                    Name = "Model Dashboard",
                    Color = "#c3cffd",
                    Icon = "task",
                    Description = "Summarize the model progress.",
                    Url = WebAppConstantValues.ModelDashboardPage,
                    ComponentType = typeof(ModelDashboardBody)
                },

                new TabbedApplication
                {
                    Name = "Subscription Dashboard",
                    Color = "#76fd98",
                    Icon = "person",
                    Description = "Table of subscribed values.",
                    Url = WebAppConstantValues.SubscriptionDashboardPage,
                    ComponentType = typeof(SubscriptionDashboardBody)
                },

                new TabbedApplication
                {
                    Name = "System Representation",
                    Color = "#a7f876",
                    Icon = "fork",
                    Description = "Represent relations between elements.",
                    Url = WebAppConstantValues.SystemRepresentationPage,
                    ComponentType = typeof(SystemRepresentationBody)
                },

                new Application
                {
                    Name = "Requirement Management",
                    Color = "#fda966",
                    Icon = "link-intact",
                    Description = $"Edit requirements in the model.{Environment.NewLine}Under Development",
                    IsDisabled = true,
                    Url = WebAppConstantValues.RequirementManagementPage
                },

                new Application
                {
                    Name = "Budget Editor",
                    Color = "#fc3a1aad",
                    Icon = "brush",
                    Description = $"Create budget tables.{Environment.NewLine}Under Development",
                    IsDisabled = true,
                    Url = WebAppConstantValues.BudgetEditorPage
                },

                new TabbedApplication
                {
                    Name = "3D Viewer",
                    Color = "#76fd98",
                    Icon = "eye",
                    Description = "Show 3D Viewer",
                    Url = WebAppConstantValues.ViewerPage,
                    ComponentType = typeof(ViewerBody)
                },

                new Application
                {
                    Name = "Reference Data",
                    Color = "#fc3a1aad",
                    Icon = "file",
                    Description = "Visualize reference data",
                    Url = WebAppConstantValues.ReferenceDataPage
                },

                new Application
                {
                    Name = "Server Administration",
                    Color = "#fc3a1aad",
                    Icon = "folder",
                    Description = "Visualize site directory data",
                    Url = WebAppConstantValues.SiteDirectoryPage
                },

                new TabbedApplication
                {
                    Name = "Engineering Model",
                    Color = "#c3cffd",
                    Icon = "cog",
                    Description = "Visualize the engineering model data",
                    Url = WebAppConstantValues.EngineeringModelPage,
                    ComponentType = typeof(EngineeringModelBody)
                },

                new TabbedApplication
                {
                    Name = "Model Editor",
                    Color = "#76fd98",
                    Icon = "box",
                    Description = "Populate model",
                    Url = WebAppConstantValues.ModelEditorPage,
                    ComponentType = typeof(ElementDefinitionTable)
                },

                new TabbedApplication()
                {
                    Name = "Book Editor",
                    Color = "#76fd98",
                    Icon = "book",
                    Description = "Manage books",
                    Url = WebAppConstantValues.BookEditorPage,
                    ComponentType = typeof(BookEditorBody)
                },

                new Application
                {
                    Name = "Tabs",
                    Url = WebAppConstantValues.TabsPage,
                    Color = "#76fd98",
                    Icon = "list",
                    Description = "Access applications via tabs",
                }
            ];

            foreach (var tabbedApplication in applications.OfType<TabbedApplication>())
            {
                tabbedApplication.ResolveTypesProperties();
            }

            return applications;
        }
    }
}
