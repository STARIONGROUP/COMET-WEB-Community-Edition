// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterEditor.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Pages.ParameterEditor
{
    using COMETwebapp.ViewModels.Pages.ParameterEditor;

    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal;
    using CDP4Dal.Events;
    
    using COMETwebapp.SessionManagement;
    
    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// Class for the <see cref="ParameterEditor"/> razor component
    /// </summary>
    public partial class ParameterEditor
    {
        /// <summary>
        /// Gets or sets the <see cref="IParameterEditorViewModel"/>
        /// </summary>
        [Inject]
        public IParameterEditorViewModel ViewModel { get; set; }

        [Parameter]
        [SupplyParameterFromQuery]
        public Guid FilterElementBase { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        private ElementBase SelectedElement { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        private bool IsOwnedParameters { get; set; } = true;

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        private string ParameterTypeSelected { get; set; }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        private string OptionSelected { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        private string StateSelected { get; set; }

        /// <summary>
        /// Listeners for the components to update it with ISession
        /// </summary>
        private Dictionary<string, IDisposable> listeners = new ();

        /// <summary>
        /// All ParameterType names in the model
        /// </summary>
        public List<string> ParameterTypeNames = new();

        /// <summary>
        /// Initialize component at first render and after session update
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ViewModel.InitializeViewModel();
            this.WhenAnyValue(x => x.ViewModel.FilteredElements.CountChanged).Subscribe(_ => this.InvokeAsync(this.StateHasChanged));

            this.WhenAnyValue(x => x.ViewModel.SelectedElementFilter,
                x => x.ViewModel.SelectedOptionFilter,
                x => x.ViewModel.SelectedParameterTypeFilter,
                x => x.ViewModel.SelectedStateFilter).Subscribe(_ => this.ViewModel.ApplyFilters(this.ViewModel.Elements));
        }
    }
}
