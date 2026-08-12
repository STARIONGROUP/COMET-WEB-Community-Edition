// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Applications.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Model
{
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Components.ModelDashboard;
    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.Components.ReferenceData;
    using COMETwebapp.Components.RelationshipMatrix;
    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.Components.SiteDirectory;
    using COMETwebapp.Components.SubscriptionDashboard;
    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.Components.Viewer;
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
                    Name = "Model Dashboard",
                    Color = "#c3cffd",
                    Icon = IconName.PieChart,
                    Description = "Summarize the model progress.",
                    Url = WebAppConstantValues.ModelDashboardPage,
                    ComponentType = typeof(ModelDashboardBody),
                    PageIntroSummary = "See how complete this iteration is and where the gaps are.",
                    PageIntroPoints = 
                    [
                        "Filter progress by option, finite state and parameter type",
                        "Track how many parameter values are published, set or still missing",
                        "See the same breakdown per element",
                        "Click a \"to do\" figure to jump straight to those parameters in the Parameter Editor"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Subscription Dashboard",
                    Color = "#76fd98",
                    Icon = IconName.Activity,
                    Description = "Table of subscribed values.",
                    Url = WebAppConstantValues.SubscriptionDashboardPage,
                    ComponentType = typeof(SubscriptionDashboardBody),
                    PageIntroSummary = "Track the parameter values your domain subscribes to, and the values other domains expect from you.",
                    PageIntroPoints = 
                    [
                        "Review the values you subscribe to and see when they change",
                        "See which domains subscribe to parameters your domain owns",
                        "Filter by parameter type and option",
                        "Click a missing value to open it in the Parameter Editor"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Requirement Management",
                    Color = "#fda966",
                    Icon = IconName.FileText,
                    Description = "View and edit requirements as documents.",
                    Url = WebAppConstantValues.RequirementManagementPage,
                    ComponentType = typeof(RequirementsEditorBody),
                    PageIntroSummary = "Read and edit the requirement specifications of this iteration as a document.",
                    PageIntroPoints = 
                    [
                        "Search definitions and filter by owner or category",
                        "Create and edit specifications, groups and requirements",
                        "Show or hide owner, category, values, constraints and traceability",
                        "Switch layout and navigate long specifications from the table of contents"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Relationship Matrix",
                    Color = "#76b8fc",
                    Icon = IconName.Grid,
                    Description = "Create and browse relationships between categorized things as a matrix.",
                    Url = WebAppConstantValues.RelationshipMatrixPage,
                    ComponentType = typeof(RelationshipMatrixBody),
                    PageIntroSummary = "Create and inspect binary relationships between two sets of categorized things.",
                    PageIntroPoints = 
                    [
                        "Configure what fills the rows and the columns, then pick a relationship rule",
                        "Create and remove relationships from the matrix itself",
                        "Show directionality, hide unrelated rows, or swap the axes",
                        "Export the matrix to Excel, and save configurations to reuse later"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Model Editor",
                    Color = "#76fd98",
                    Icon = IconName.Layers,
                    Description = "Populate model",
                    Url = WebAppConstantValues.ModelEditorPage,
                    ComponentType = typeof(ModelEditor),
                    PageIntroSummary = "Populate this iteration by building element definitions and copying them between models.",
                    PageIntroPoints = 
                    [
                        "Drag element definitions from the source tree into the target model",
                        "Hold Ctrl, Shift or both while dropping to change what the copy keeps",
                        "Select any element to add, edit or subscribe to its parameters in the details panel",
                        "Choose a different source model in the left panel to copy from"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Parameter Editor",
                    Color = "#76b8fc",
                    Icon = IconName.Layout,
                    Description = "Table of element usages with their associated parameters.",
                    Url = WebAppConstantValues.ParameterEditorPage,
                    ComponentType = typeof(ParameterEditorBody),
                    PageIntroSummary = "Review and edit the parameter values of this iteration.",
                    PageIntroPoints = 
                    [
                        "Filter by element, parameter type, category or option",
                        "Edit values inline \u2014 the count shows how many of the total you are viewing",
                        "Use Batch Edit to apply one value across many parameters at once",
                        "Switch to only the parameters owned by your active domain"
                    ]
                },

                new TabbedApplication
                {
                    Name = "System Representation",
                    Color = "#a7f876",
                    Icon = IconName.Share,
                    Description = "Represent relations between elements.",
                    Url = WebAppConstantValues.SystemRepresentationPage,
                    ComponentType = typeof(SystemRepresentationBody),
                    PageIntroSummary = "Browse the product tree of this iteration and inspect how elements are composed.",
                    PageIntroPoints = 
                    [
                        "Pick an option to see the tree resolved for it",
                        "Expand the tree to follow the containment of element usages",
                        "Select any element to view its parameters and details",
                        "Drag the divider to give the tree or the details panel more room"
                    ]
                },

                new TabbedApplication
                {
                    Name = "3D Viewer",
                    Color = "#76fd98",
                    Icon = IconName.Package,
                    Description = "Show 3D Viewer",
                    Url = WebAppConstantValues.ViewerPage,
                    ComponentType = typeof(ViewerBody),
                    PageIntroSummary = "See this iteration's elements rendered in 3D from their shape and position parameters.",
                    PageIntroPoints = 
                    [
                        "Pick an option and finite states to change what is rendered",
                        "Select a node in the tree or a shape in the scene to inspect it",
                        "Review the selected element's parameter values alongside the view",
                        "Show and hide parts of the product tree to isolate what you are looking at"
                    ]
                },

                new Application
                {
                    Name = "Budget Editor",
                    Color = "#fc3a1aad",
                    Icon = IconName.PieChart,
                    Description = $"Create budget tables.{Environment.NewLine}Under Development",
                    IsDisabled = true,
                    Url = WebAppConstantValues.BudgetEditorPage,
                    PageIntroSummary = "Budget tables are under development and not available yet."
                },

                new TabbedApplication
                {
                    Name = "Book Editor",
                    Color = "#76fd98",
                    Icon = IconName.Book,
                    Description = "Manage books",
                    Url = WebAppConstantValues.BookEditorPage,
                    ComponentType = typeof(BookEditorBody),
                    PageIntroSummary = "Capture engineering notes as a structured book for this model.",
                    PageIntroPoints = 
                    [
                        "Work left to right: books hold sections, sections hold pages, pages hold notes",
                        "Add, rename or delete an item from the buttons in each column",
                        "Select an item to reveal its children in the next column",
                        "Collapse a column to give the ones on its right more room"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Engineering Model",
                    Color = "#c3cffd",
                    Icon = IconName.Settings,
                    Description = "Visualize the engineering model data",
                    Url = WebAppConstantValues.EngineeringModelPage,
                    ComponentType = typeof(EngineeringModelBody),
                    PageIntroSummary = "Manage the setup data that belongs to this engineering model and iteration.",
                    PageIntroPoints = 
                    [
                        "Options \u2014 define the options of this iteration",
                        "Publications \u2014 publish parameter values and review earlier publications",
                        "Common File Store \u2014 share files across the whole model",
                        "Domain File Store \u2014 keep files scoped to a single domain of expertise"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Reference Data",
                    Color = "#fc3a1aad",
                    Icon = IconName.File,
                    Description = "Visualize reference data",
                    Url = WebAppConstantValues.ReferenceDataPage,
                    ComponentType = typeof(ReferenceDataBody),
                    PageIntroSummary = "Browse the reference data libraries this server makes available to your models.",
                    PageIntroPoints = 
                    [
                        "Parameter Types \u2014 the quantities and qualities that parameters can hold",
                        "Measurement Scales and Units \u2014 how those values are expressed",
                        "Categories \u2014 the classifications used to group and filter things",
                        "Use the toolbar to switch between the four libraries"
                    ]
                },

                new TabbedApplication
                {
                    Name = "Server Administration",
                    Color = "#fc3a1aad",
                    Icon = IconName.Server,
                    Description = "Visualize site directory data",
                    Url = WebAppConstantValues.SiteDirectoryPage,
                    ComponentType = typeof(SiteDirectoryBody),
                    PageIntroSummary = "Administer the models, people and permissions held in this server's site directory.",
                    PageIntroPoints = 
                    [
                        "Models \u2014 create engineering models and manage their participants",
                        "Domains and Organizations \u2014 the domains of expertise and organizations available",
                        "User Management \u2014 the people who can sign in to this server",
                        "Person and Participant Roles \u2014 what those people are permitted to do"
                    ]
                },

                new Application
                {
                    Name = "Tabs",
                    Url = WebAppConstantValues.TabsPage,
                    Color = "#76fd98",
                    Icon = IconName.List,
                    Description = "Access applications via tabs",
                    PageIntroSummary = "Work on several applications side by side without losing your place.",
                    PageIntroPoints = 
                    [
                        "Use + to open another application in a new tab",
                        "Drag a tab to reorder it, or drag it into the other panel",
                        "Use the split-view button to open a second panel",
                        "The badge in the tab bar shows your active domain \u2014 click it to switch"
                    ]
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
