// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBody.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.Components.ParameterEditor
{
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="ParameterEditorBody" /> component
    /// </summary>
    public partial class ParameterEditorBody
    {
        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.OptionSelector.SelectedOption,
                    x => x.ViewModel.ParameterTypeSelector.SelectedParameterType,
                    x => x.ViewModel.ElementSelector.SelectedElementBase,
                    x => x.ViewModel.IsOwnedParameters)
                .Subscribe(_ => this.UpdateUrl()));
        }

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

            if (parameters.TryGetValue(QueryKeys.ParameterKey, out var parameter))
            {
                this.ViewModel.ParameterTypeSelector.SelectedParameterType = this.ViewModel.ParameterTypeSelector.AvailableParameterTypes.FirstOrDefault(x => x.Iid == parameter.FromShortGuid());
            }
        }

        /// <summary>
        /// Sets the url of the <see cref="NavigationManager" /> based on the current values
        /// </summary>
        private void UpdateUrl()
        {
            var additionalParameters = new Dictionary<string, string>();

            if (this.ViewModel.ElementSelector.SelectedElementBase != null)
            {
                additionalParameters["element"] = this.ViewModel.ElementSelector.SelectedElementBase.Iid.ToShortGuid();
            }

            if (this.ViewModel.OptionSelector.SelectedOption != null)
            {
                additionalParameters["option"] = this.ViewModel.OptionSelector.SelectedOption.Iid.ToShortGuid();
            }

            if (this.ViewModel.ParameterTypeSelector.SelectedParameterType != null)
            {
                additionalParameters["parameter"] = this.ViewModel.ParameterTypeSelector.SelectedParameterType.Iid.ToShortGuid();
            }

            if (this.ViewModel.IsOwnedParameters)
            {
                additionalParameters["owned"] = this.ViewModel.IsOwnedParameters.ToString();
            }

            this.UpdateUrlWithParameters(additionalParameters, WebAppConstantValues.ParameterEditorPage);
        }
    }
}
