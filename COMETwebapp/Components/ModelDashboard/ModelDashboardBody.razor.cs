// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelDashboardBody.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelDashboard
{
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Core component for the Model Dashboard application
    /// </summary>
    public partial class ModelDashboardBody
    {
        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue(QueryKeys.OptionKey, out var option))
            {
                this.ViewModel.OptionSelector.SelectedOption = this.ViewModel.OptionSelector.AvailableOptions.FirstOrDefault(x => x.Iid == option.FromShortGuid());
            }

            if (parameters.TryGetValue(QueryKeys.StateKey, out var state))
            {
                this.ViewModel.FiniteStateSelector.SelectedActualFiniteState = this.ViewModel.FiniteStateSelector.AvailableFiniteStates.FirstOrDefault(x => x.Iid == state.FromShortGuid());
            }

            if (parameters.TryGetValue(QueryKeys.ParameterKey, out var parameter))
            {
                this.ViewModel.ParameterTypeSelector.SelectedParameterType = this.ViewModel.ParameterTypeSelector.AvailableParameterTypes.FirstOrDefault(x => x.Iid == parameter.FromShortGuid());
            }
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.OptionSelector.SelectedOption,
                    x => x.ViewModel.FiniteStateSelector.SelectedActualFiniteState,
                    x => x.ViewModel.ParameterTypeSelector.SelectedParameterType)
                .Subscribe(_ => this.UpdateUrl()));
        }

        /// <summary>
        /// Sets the url of the <see cref="NavigationManager" /> based on the current values
        /// </summary>
        /// <param name="pageName">The name of the page to redirect to</param>
        private void UpdateUrl(string pageName = WebAppConstantValues.ModelDashboardPage)
        {
            var additionalParameters = new Dictionary<string, string>();

            if (this.ViewModel.OptionSelector.SelectedOption != null)
            {
                additionalParameters[QueryKeys.OptionKey] = this.ViewModel.OptionSelector.SelectedOption.Iid.ToShortGuid();
            }

            if (this.ViewModel.FiniteStateSelector.SelectedActualFiniteState != null)
            {
                additionalParameters[QueryKeys.StateKey] = this.ViewModel.FiniteStateSelector.SelectedActualFiniteState.Iid.ToShortGuid();
            }

            if (this.ViewModel.ParameterTypeSelector.SelectedParameterType != null)
            {
                additionalParameters[QueryKeys.ParameterKey] = this.ViewModel.ParameterTypeSelector.SelectedParameterType.Iid.ToShortGuid();
            }

            this.ViewModel.UpdateDashboards();
            this.UpdateUrlWithParameters(additionalParameters, pageName);
        }

        /// <summary>
        /// Redirects the user to the ParameterEditor page with the current selected filters
        /// </summary>
        private void RedirectToParameterEditor()
        {
            this.UpdateUrl(WebAppConstantValues.ParameterEditorPage);
        }
    }
}
