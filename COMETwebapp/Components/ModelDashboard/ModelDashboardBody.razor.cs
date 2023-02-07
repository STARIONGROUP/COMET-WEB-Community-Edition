// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelDashboardBody.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.ModelDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Core component for the Model Dashboard application
    /// </summary>
    public partial class ModelDashboardBody
    {
        /// <summary>
        /// The initial <see cref="Option" />
        /// </summary>
        [Parameter]
        public Option InitialOption { get; set; }

        /// <summary>
        /// The initial <see cref="ActualFiniteState" />
        /// </summary>
        [Parameter]
        public ActualFiniteState InitialActualFiniteState { get; set; }

        /// <summary>
        /// The initial <see cref="ParameterType" />
        /// </summary>
        [Parameter]
        public ParameterType InitialParameterType { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.ViewModel.OptionSelector.SelectedOption = this.InitialOption;
            this.ViewModel.FiniteStateSelector.SelectedActualFiniteState = this.InitialActualFiniteState;
            this.ViewModel.ParameterTypeSelector.SelectedParameterType = this.InitialParameterType;
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.OptionSelector.SelectedOption,
                    x => x.ViewModel.FiniteStateSelector.SelectedActualFiniteState,
                    x => x.ViewModel.ParameterTypeSelector.SelectedParameterType)
                .Subscribe(_ => this.UpdateUrl()));
        }

        /// <summary>
        /// Sets the url of the <see cref="NavigationManager" /> based on the current values
        /// </summary>
        /// <param name="pageName">The name of the page to redirect to</param>
        private void UpdateUrl(string pageName = "ModelDashboard")
        {
            var additionalParameters = new Dictionary<string, string>();

            if (this.ViewModel.OptionSelector.SelectedOption != null)
            {
                additionalParameters["option"] = this.ViewModel.OptionSelector.SelectedOption.Iid.ToString();
            }

            if (this.ViewModel.FiniteStateSelector.SelectedActualFiniteState != null)
            {
                additionalParameters["state"] = this.ViewModel.FiniteStateSelector.SelectedActualFiniteState.Iid.ToString();
            }

            if (this.ViewModel.ParameterTypeSelector.SelectedParameterType != null)
            {
                additionalParameters["parameter"] = this.ViewModel.ParameterTypeSelector.SelectedParameterType.Iid.ToString();
            }

            this.UpdateUrlWithParameters(additionalParameters, pageName);
        }

        /// <summary>
        /// Redirects the user to the ParameterEditor page with the current selected filters
        /// </summary>
        private void RedirectToParameterEditor()
        {
            this.UpdateUrl("ParameterEditor");
        }
    }
}
