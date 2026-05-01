// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBody.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ParameterEditor
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="ParameterEditorBody" /> component
    /// </summary>
    public partial class ParameterEditorBody
    {
        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.OptionSelector.SelectedOption,
                    x => x.ViewModel.ParameterTypeSelector.SelectedParameterTypes,
                    x => x.ViewModel.ElementSelector.SelectedElementBase,
                    x => x.ViewModel.CategorySelector.SelectedCategories,
                    x => x.ViewModel.IsOwnedParameters)
                .Subscribe(_ => this.UpdateUrl()));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.BatchParameterEditorViewModel.IsLoading).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
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

            if (parameters.TryGetValue(QueryKeys.ParametersKey, out var parametersValue) && !string.IsNullOrWhiteSpace(parametersValue))
            {
                var ids = parametersValue
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.FromShortGuid())
                    .ToHashSet();

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

            if (parameters.TryGetValue(QueryKeys.CategoriesKey, out var categoriesValue) && !string.IsNullOrWhiteSpace(categoriesValue))
            {
                var ids = categoriesValue
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.FromShortGuid())
                    .ToHashSet();

                this.ViewModel.CategorySelector.SelectedCategories = this.ViewModel.CategorySelector.AvailableCategories
                    .Where(x => ids.Contains(x.Iid))
                    .ToList();
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

            var selectedParameterTypes = this.ViewModel.ParameterTypeSelector.SelectedParameterTypes?.ToList() ?? new List<ParameterType>();

            if (selectedParameterTypes.Count > 0)
            {
                additionalParameters[QueryKeys.ParametersKey] = string.Join(",", selectedParameterTypes.Select(x => x.Iid.ToShortGuid()));
            }

            var selectedCategories = this.ViewModel.CategorySelector.SelectedCategories?.ToList() ?? new List<Category>();

            if (selectedCategories.Count > 0)
            {
                additionalParameters[QueryKeys.CategoriesKey] = string.Join(",", selectedCategories.Select(x => x.Iid.ToShortGuid()));
            }

            if (this.ViewModel.IsOwnedParameters)
            {
                additionalParameters["owned"] = this.ViewModel.IsOwnedParameters.ToString();
            }

            this.ViewModel.ApplyFilters();
            this.UpdateUrlWithParameters(additionalParameters, WebAppConstantValues.ParameterEditorPage);
        }
    }
}
