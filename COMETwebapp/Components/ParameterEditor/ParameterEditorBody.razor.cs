// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBody.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

using ReactiveUI;

namespace COMETwebapp.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParameterEditorBody"/> component
    /// </summary>
    public partial class ParameterEditorBody
    {
        /// <summary>
        /// The initial <see cref="ElementBase" />
        /// </summary>
        [Parameter]
        public ElementBase InitialElementBase { get; set; }

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
            this.ViewModel.InitializeViewModel();

            this.Disposables.Add(
                this.WhenAnyValue(x => x.ViewModel.FilteredElements.CountChanged)
                    .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.ElementSelector.SelectedElementBase,
                x => x.ViewModel.OptionSelector.SelectedOption,
                x => x.ViewModel.FiniteStateSelector.SelectedActualFiniteState,
                x => x.ViewModel.ParameterTypeSelector.SelectedParameterType,
                x => x.ViewModel.IsOwnedParameters).Subscribe(_ => this.ViewModel.ApplyFilters(this.ViewModel.Elements)));

            this.ViewModel.ElementSelector.SelectedElementBase = this.InitialElementBase;
            this.ViewModel.OptionSelector.SelectedOption = this.InitialOption;
            this.ViewModel.FiniteStateSelector.SelectedActualFiniteState = this.InitialActualFiniteState;
            this.ViewModel.ParameterTypeSelector.SelectedParameterType = this.InitialParameterType;
        }
    }
}
