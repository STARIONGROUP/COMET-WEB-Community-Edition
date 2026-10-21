// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameter.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelEditor
{
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Dialog that edits an existing <see cref="CDP4Common.EngineeringModelData.Parameter" /> or
    /// <see cref="CDP4Common.EngineeringModelData.ParameterOverride" />: a Basic metadata tab, an editable Values
    /// tab (all parameter types), and a read-only Subscriptions tab. Opened from a parameter card on the element
    /// details panel.
    /// </summary>
    public partial class EditParameter
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. The dialog still opens so the parameter can be inspected,
        /// but the OK button that would commit the edit is withdrawn
        /// </summary>
        private bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Callback closing the component-value edit popup (the sampled-function / compound table editor).
        /// </summary>
        private EventCallback closeComponentEditor;

        /// <summary>
        /// Gets or sets the <see cref="IEditParameterViewModel" /> that drives this dialog.
        /// </summary>
        [Parameter]
        public IEditParameterViewModel ViewModel { get; set; }

        /// <summary>
        /// Subscribes to <see cref="IEditParameterViewModel.ValuesEditable" /> so the dialog re-renders (hiding or
        /// showing the values grid) when the owner / option-dependence / state-dependence changes — the owner change
        /// comes from a child selector component, so a plain <c>@bind</c> re-render is not enough on its own.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override Task OnInitializedAsync()
        {
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.ValuesEditable, x => x.ViewModel.IsOnComponentEditMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.closeComponentEditor = new EventCallbackFactory().Create(this, () => { this.ViewModel.IsOnComponentEditMode = false; });

            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Gets a value indicating whether the Option column should be shown — i.e. at least one value set applies
        /// to a specific <see cref="CDP4Common.EngineeringModelData.Option" />.
        /// </summary>
        private bool HasOptionColumn => this.ViewModel.ValueRows.Any(row => !string.IsNullOrEmpty(row.OptionName));

        /// <summary>
        /// Gets a value indicating whether the State column should be shown — i.e. at least one value set applies
        /// to a specific <see cref="CDP4Common.EngineeringModelData.ActualFiniteState" />.
        /// </summary>
        private bool HasStateColumn => this.ViewModel.ValueRows.Any(row => !string.IsNullOrEmpty(row.StateName));

        /// <summary>
        /// Gets a value indicating whether the parameter is a compound parameter type — in which case the values grid
        /// shows one row per component with the component Name / Parameter Type / Scale columns.
        /// </summary>
        private bool IsCompound => this.ViewModel.ValueRows.Any(row => !string.IsNullOrEmpty(row.Name));
    }
}
