// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConstantValues.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Utilities
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Static class to handle constant values that are shared accross the applicatio
    /// </summary>
    public static class ConstantValues
    {
        /// <summary>
        /// Label used for the published <see cref="ParameterBase" />
        /// </summary>
        public const string PublishedParameters = "Published Parameters";

        /// <summary>
        /// Label used for the publishable <see cref="ParameterBase" />
        /// </summary>
        public const string PublishableParameters = "Publishable Parameters";

        /// <summary>
        /// Label used for <see cref="ParameterBase" /> that have missing values
        /// </summary>
        public const string ParametersWithMissingValues = "Missing Values";

        /// <summary>
        /// Label used for <see cref="ParameterBase" /> that have values
        /// </summary>
        public const string ParametersWithValues = "Complete Values";

        /// <summary>
        /// Label used for <see cref="ElementBase" /> that are not used
        /// </summary>
        public const string UnusedElements = "Unused Elements";

        /// <summary>
        /// Label used for <see cref="ElementBase" /> that are used
        /// </summary>
        public const string UsedElements = "Used Elements";

        /// <summary>
        /// Label used for <see cref="ElementBase" /> that are not referenced
        /// </summary>
        public const string UnreferencedElements = "Unreferenced Elements";

        /// <summary>
        /// Label used for <see cref="ElementBase" /> that are  referenced
        /// </summary>
        public const string ReferencedElements = "Referenced Elements";

        /// <summary>
        /// The page name for the Model Dashboard
        /// </summary>
        public const string ModelDashboardPage = "ModelDashboard";

        /// <summary>
        /// The page name of the Parameter Editor
        /// </summary>
        public const string ParameterEditorPage = "ParameterEditor";

        /// <summary>
        /// The page name of the Subscription Dashboard
        /// </summary>
        public const string SubscriptionDashboardPage = "SubscriptionDashboard";

        /// <summary>
        /// The page name of the 3D Viewer
        /// </summary>
        public const string ViewerPage = "3DViewer";

        /// <summary>
        /// The page name of the System Representation
        /// </summary>
        public const string SystemRepresentationPage = "SystemRepresentation";

        /// <summary>
        /// The page name of the Requirement Management
        /// </summary>
        public const string RequirementManagementPage = "RequirementManagement";

        /// <summary>
        /// The page name of the Budget Editor
        /// </summary>
        public const string BudgetEditorPage = "BudgetEditor";
    }
}
