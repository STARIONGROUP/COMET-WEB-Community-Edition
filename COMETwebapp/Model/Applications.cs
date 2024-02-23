// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Applications.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

    using COMETwebapp.Utilities;

    /// <summary>
    /// Provides all available application contained into the current web application
    /// </summary>
    public static class Applications
    {
        /// <summary>
        /// List of <see cref="Application"/> with Name, Color, Icon and Description data
        /// </summary>
        public static List<Application> ExistingApplications => new()
        {
            new Application
			{
                Name = "Parameter Editor",
                Color = "#76b8fc",
                Icon = "spreadsheet",
                Description = "Table of element usages with their associated parameters.",
                Url = WebAppConstantValues.ParameterEditorPage
            },
            new Application
			{
                Name = "Model Dashboard",
                Color = "#c3cffd",
                Icon = "task",
                Description = "Summarize the model progress.",
                Url = WebAppConstantValues.ModelDashboardPage
            },
            new Application
			{
                Name = "Subscription Dashboard",
                Color = "#76fd98",
                Icon = "person",
                Description = "Table of subscribed values.",
                Url = WebAppConstantValues.SubscriptionDashboardPage
            },
            new Application
			{
                Name = "System Representation",
                Color = "#a7f876",
                Icon = "fork",
                Description = "Represent relations between elements.",
                Url = WebAppConstantValues.SystemRepresentationPage
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
            new Application
            {
                Name = "3D Viewer",
                Color = "#76fd98",
                Icon = "eye",
                Description = "Show 3D Viewer",
                Url = WebAppConstantValues.ViewerPage
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
                Name = "User Management",
                Color = "#76fd98",
                Icon = "people",
                Description = "Manage users",
                Url = WebAppConstantValues.UserManagementPage
            },
            new Application
            {
                Name = "Model Editor",
                Color = "#76fd98",
                Icon = "box",
                Description = "Populate model",
                Url = WebAppConstantValues.ModelEditorPage
            },
            new Application
            {
                Name = "Book Editor",
                Color = "#76fd98",
                Icon = "book",
                Description = "Manage books",
                Url = WebAppConstantValues.BookEditorPage
            }
        };
    }
}
