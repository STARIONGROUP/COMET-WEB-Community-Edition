// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBody.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Core component for the Subscription Dashboard application
    /// </summary>
    public partial class SubscriptionDashboardBody
    {
        /// <summary>
        /// The title for the <see cref="SubscribedTable"/>
        /// </summary>
        private string SubscriptionTableTitle => $"Parameters {this.ViewModel.CurrentDomain.Name} domain subscribed to";

        /// <summary>
        /// The title for the <see cref="DomainOfExpertiseSubscriptionTable"/>
        /// </summary>
        private string DomainOfExpertiseTableTitle => $"Parameters owned by {this.ViewModel.CurrentDomain.Name} domain, subscribed to by other domains";

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.OptionSelector.SelectedOptions,
                    x => x.ViewModel.ParameterTypeSelector.SelectedParameterTypes)
                .Subscribe(_ => this.UpdateUrl()));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue(QueryKeys.OptionsKey, out var optionsValue) && !string.IsNullOrWhiteSpace(optionsValue))
            {
                var ids = optionsValue.FromShortGuids();

                this.ViewModel.OptionSelector.SelectedOptions = this.ViewModel.OptionSelector.AvailableOptions
                    .Where(x => ids.Contains(x.Iid))
                    .ToList();
            }

            if (parameters.TryGetValue(QueryKeys.ParametersKey, out var parametersValue) && !string.IsNullOrWhiteSpace(parametersValue))
            {
                var ids = parametersValue.FromShortGuids();

                this.ViewModel.ParameterTypeSelector.SelectedParameterTypes = this.ViewModel.ParameterTypeSelector.AvailableParameterTypes
                    .Where(x => ids.Contains(x.Iid))
                    .ToList();
            }
            else if (parameters.TryGetValue(QueryKeys.ParameterKey, out var parameter))
            {
                var match = this.ViewModel.ParameterTypeSelector.AvailableParameterTypes.FirstOrDefault(x => x.Iid == parameter.FromShortGuid());

                if (match != null)
                {
                    this.ViewModel.ParameterTypeSelector.SelectedParameterTypes = new List<ParameterType> { match };
                }
            }
        }

        /// <summary>
        /// Sets the url of the <see cref="NavigationManager" /> based on the current values
        /// </summary>
        private void UpdateUrl()
        {
            var additionalParameters = new Dictionary<string, string>();

            var selectedOptions = this.ViewModel.OptionSelector.SelectedOptions?.ToList() ?? new List<Option>();

            if (selectedOptions.Count > 0)
            {
                additionalParameters[QueryKeys.OptionsKey] = string.Join(",", selectedOptions.Select(x => x.Iid.ToShortGuid()));
            }

            var selectedParameterTypes = this.ViewModel.ParameterTypeSelector.SelectedParameterTypes?.ToList() ?? new List<ParameterType>();

            if (selectedParameterTypes.Count > 0)
            {
                additionalParameters[QueryKeys.ParametersKey] = string.Join(",", selectedParameterTypes.Select(x => x.Iid.ToShortGuid()));
            }

            this.ViewModel.UpdateTables();
            this.UpdateUrlWithParameters(additionalParameters, WebAppConstantValues.SubscriptionDashboardPage);
        }

        /// <summary>
        /// Redirect to the <see cref="ParameterEditor"/> page to complete missing values
        /// </summary>
        /// <param name="parameterOrOverrideBase">The <see cref="ParameterOrOverrideBase"/> to complete</param>
        private void RedirectToParameterEditor(ParameterOrOverrideBase parameterOrOverrideBase)
        {
            var additionalParameters = new Dictionary<string, string>
            {
                [QueryKeys.ParameterKey] = parameterOrOverrideBase.ParameterType.Iid.ToShortGuid()
            };

            this.UpdateUrlWithParameters(additionalParameters, WebAppConstantValues.ParameterEditorPage);
        }
    }
}
