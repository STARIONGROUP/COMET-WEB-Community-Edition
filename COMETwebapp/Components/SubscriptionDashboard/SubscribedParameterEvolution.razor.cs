﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscribedParameterEvolution.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that display the evolution of a <see cref="ParameterSubscriptionRowViewModel" />
    /// </summary>
    public partial class SubscribedParameterEvolution
    {
        /// <summary>
        /// The <see cref="ParameterSubscriptionRowViewModel" /> to display the evolution
        /// </summary>
        [Parameter]
        public ParameterSubscriptionRowViewModel ParameterSubscriptionRow { get; set; }

        /// <summary>
        /// A collection of <see cref="RevisionHistory" />
        /// </summary>
        public IEnumerable<RevisionHistory> RevisionHistories { get; private set; }

        /// <summary>
        /// The name of the associated <see cref="ElementBase" />
        /// </summary>
        public string ElementName { get; private set; }

        /// <summary>
        /// The short name of the associated <see cref="MeasurementScale" />
        /// </summary>
        public string ScaleShortName { get; private set; }

        /// <summary>
        /// The name of the associated <see cref="ParameterSubscription" />
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.ParameterSubscriptionRow != null)
            {
                this.ComputeEvolutionGraphData();
            }
        }

        /// <summary>
        /// Computes the data to display on the evolution graph
        /// </summary>
        private void ComputeEvolutionGraphData()
        {
            this.RevisionHistories = this.ParameterSubscriptionRow.Changes.Select(valueSetRevision =>
                new RevisionHistory(valueSetRevision.Key,valueSetRevision.Value));

            this.ParameterName = this.ParameterSubscriptionRow.ParameterName;
            this.ScaleShortName = this.ParameterSubscriptionRow.Parameter.Scale?.ShortName;
            this.ElementName = this.ParameterSubscriptionRow.ElementName;
        }

        /// <summary>
        /// Customize the point Label
        /// </summary>
        /// <param name="pointSettings">The <see cref="ChartSeriesPointCustomizationSettings"/></param>
        private void PreparePointLabel(ChartSeriesPointCustomizationSettings pointSettings)
        {
            var argument = (string)pointSettings.Point.Argument;

            if (this.RevisionHistories.FirstOrDefault(x => x.RevisionNumber == argument) is RevisionHistory revisionHistory)
            {
                pointSettings.PointLabel.Visible = true;

                if (revisionHistory.ActualValue == null)
                {
                    pointSettings.PointLabel.FormatPattern ="-";
                }
            }
        }
    }
}
