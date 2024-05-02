// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterEvolution.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows;
    
    using DevExpress.Blazor;
    
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that display the evolution of a <see cref="ParameterSubscriptionRowViewModel" /> for a <see cref="CompoundParameterType" />
    /// </summary>
    public partial class CompoundParameterEvolution
    {
        /// <summary>
        /// The <see cref="ParameterSubscriptionRowViewModel" /> to display the evolution
        /// </summary>
        [Parameter]
        public ParameterSubscriptionRowViewModel ParameterSubscriptionRow { get; set; }

        /// <summary>
        /// The name of selected dimension
        /// </summary>
        private string selectedDimension = "1";

        /// <summary>
        /// A collection of <see cref="ParameterSubscriptionRowViewModel" />
        /// </summary>
        public IEnumerable<ParameterSubscriptionRowViewModel> ParameterSubscriptions { get; private set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.ParameterSubscriptionRow != null)
            {
                this.CreateParameterSubscriptions();
            }
        }

        /// <summary>
        /// Creates parameters subscription
        /// </summary>
        private void CreateParameterSubscriptions()
        {
            var parameterSubscriptions = new List<ParameterSubscriptionRowViewModel>();

            var compoundParameterType = (CompoundParameterType)this.ParameterSubscriptionRow.Parameter.ParameterType;

            for (var parameterTypeComponent = 0; parameterTypeComponent < compoundParameterType.Component.Count; parameterTypeComponent++)
            {
                var parameterType = compoundParameterType.Component[parameterTypeComponent].ParameterType;

                var containerParameter = new Parameter()
                {
                    Iid = Guid.NewGuid(),
                    ParameterType = parameterType
                };

                var changes = new Dictionary<int, ValueArray<string>>();

                foreach (var kvp in this.ParameterSubscriptionRow.Changes)
                {
                    var valueArray = new ValueArray<string>(new[] { kvp.Value[parameterTypeComponent] });
                    changes.Add(kvp.Key, valueArray);
                }

                var parameterSubscriptionViewModel = new ParameterSubscriptionRowViewModel()
                {
                    Parameter = containerParameter,
                    Changes = changes
                };

                parameterSubscriptions.Add(parameterSubscriptionViewModel);
            }

            this.ParameterSubscriptions = parameterSubscriptions;
        }

        /// <summary>
        /// Method invoked to set the selected dimension from toolbar
        /// </summary>
        /// <param name="e"></param>
        void OnItemClick(ToolbarItemClickEventArgs e)
        {
            this.selectedDimension = e.ItemName;
        }
    }
}
