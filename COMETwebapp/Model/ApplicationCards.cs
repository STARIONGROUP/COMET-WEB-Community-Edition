// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationCards.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace COMETwebapp.Model
{
    using COMETwebapp.Utilities;

    /// <summary>
    /// Provides all available application contained into the current web application
    /// </summary>
    public static class ApplicationCards
    {
        /// <summary>
        /// List of application cards with Name, Color, Icon and Description data
        /// </summary>
        public static List<Card> Cards => new()
        {
            new Card
            {
                Name = "Parameter Editor",
                Color = "#76b8fc",
                Icon = "spreadsheet",
                Description = "Table of element usages with their associated parameters.",
                Url = ConstantValues.ParameterEditorPage
            },
            new Card
            {
                Name = "Model Dashboard",
                Color = "#c3cffd",
                Icon = "task",
                Description = "Summarize the model progress.",
                Url = ConstantValues.ModelDashboardPage
            },
            new Card
            {
                Name = "Subscription Dashboard",
                Color = "#76fd98",
                Icon = "person",
                Description = "Table of subscribed values.",
                Url = ConstantValues.SubscriptionDashboardPage
            },
            new Card
            {
                Name = "System Representation",
                Color = "#a7f876",
                Icon = "fork",
                Description = "Represent relations between elements.",
                Url = ConstantValues.SystemRepresentationPage
            },
            new Card
            {
                Name = "Requirement Management",
                Color = "#fda966",
                Icon = "link-intact",
                Description = "Edit requirements in the model.",
                Url = ConstantValues.RequirementManagementPage
            },
            new Card
            {
                Name = "Budget Editor",
                Color = "#fc3a1aad",
                Icon = "brush",
                Description = "Create budget tables.",
                Url = ConstantValues.BudgetEditorPage
            },
            new Card
            {
                Name = "3D Viewer",
                Color = "#76fd98",
                Icon = "eye",
                Description = "Show 3D Viewer",
                Url = ConstantValues.ViewerPage
            }
        };
    }
}
