// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IEditParameterViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Interface for the <see cref="EditParameterViewModel" /> that drives the Edit Parameter dialog opened from a
    /// parameter card on the element details panel. Supports editing an existing <see cref="Parameter" /> or a
    /// <see cref="ParameterOverride" />: its Basic metadata, its per-type values, and a read-only view of its
    /// subscriptions.
    /// </summary>
    public interface IEditParameterViewModel
    {
        /// <summary>
        /// Gets the working clone of the <see cref="ParameterOrOverrideBase" /> being edited.
        /// </summary>
        ParameterOrOverrideBase Parameter { get; }

        /// <summary>
        /// Gets a value indicating whether the edited thing is a <see cref="Parameter" /> (as opposed to a
        /// <see cref="ParameterOverride" />). Only a <see cref="Parameter" /> exposes group / option-dependence /
        /// state-dependence / expects-override fields.
        /// </summary>
        bool IsParameter { get; }

        /// <summary>
        /// Gets the working clone typed as a <see cref="Parameter" />, or <c>null</c> when a
        /// <see cref="ParameterOverride" /> is being edited.
        /// </summary>
        Parameter ParameterAsParameter { get; }

        /// <summary>
        /// Gets the display name of the edited parameter type.
        /// </summary>
        string ParameterTypeName { get; }

        /// <summary>
        /// Gets a value indicating whether the value sets may be edited (turns <c>false</c> once the option or state
        /// dependence, or the owner, is changed).
        /// </summary>
        bool ValuesEditable { get; }

        /// <summary>
        /// Gets the message explaining why the values grid is temporarily disabled, or an empty string when editable.
        /// </summary>
        string ValuesDisabledReason { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the component-value edit popup (the sampled-function / compound
        /// table editor) is visible.
        /// </summary>
        bool IsOnComponentEditMode { get; set; }

        /// <summary>
        /// Gets the component value editor to show in the <see cref="IsOnComponentEditMode" /> popup.
        /// </summary>
        IHaveComponentParameterTypeEditor HaveComponentParameterTypeEditorViewModel { get; }

        /// <summary>
        /// Gets or sets whether the edited <see cref="Parameter" /> is option-dependent.
        /// </summary>
        bool IsOptionDependent { get; set; }

        /// <summary>
        /// Gets or sets the edited <see cref="Parameter" />'s state dependence.
        /// </summary>
        ActualFiniteStateList SelectedStateDependence { get; set; }

        /// <summary>
        /// Gets the model code of the edited parameter.
        /// </summary>
        string ModelCode { get; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" /> driving the Owner selector.
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IMeasurementScaleSelectorViewModel" /> driving the Scale selector (quantity kinds only).
        /// </summary>
        IMeasurementScaleSelectorViewModel MeasurementScaleSelectorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="ActualFiniteStateList" />s selectable for the State Dependence field.
        /// </summary>
        IEnumerable<ActualFiniteStateList> PossibleFiniteStates { get; }

        /// <summary>
        /// Gets the <see cref="ParameterGroup" />s selectable for the Group field.
        /// </summary>
        IEnumerable<ParameterGroup> ParameterGroups { get; }

        /// <summary>
        /// Gets the editable value-set rows shown on the Values tab.
        /// </summary>
        IReadOnlyList<EditParameterValueSetRowViewModel> ValueRows { get; }

        /// <summary>
        /// Gets the read-only subscription rows shown on the Subscriptions tab.
        /// </summary>
        IReadOnlyList<ParameterSubscriptionInfoRowViewModel> SubscriptionRows { get; }

        /// <summary>
        /// Gets the callback invoked once the dialog is done (after a successful save or a cancel) so the host can
        /// close the popup.
        /// </summary>
        EventCallback OnParameterEdited { get; set; }

        /// <summary>
        /// Initializes the view model for the supplied <see cref="ParameterOrOverrideBase" />, cloning it and
        /// populating the selectors, value rows and subscription rows.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> to edit.</param>
        /// <param name="iteration">The <see cref="Iteration" /> the parameter belongs to.</param>
        /// <param name="currentDomain">The currently logged-in <see cref="DomainOfExpertise" />.</param>
        void SetParameter(ParameterOrOverrideBase parameter, Iteration iteration, DomainOfExpertise currentDomain);

        /// <summary>
        /// Commits the pending metadata and value edits to the session and closes the dialog.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous save operation.</returns>
        Task SaveAsync();

        /// <summary>
        /// Discards the pending edits and closes the dialog.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous cancel operation.</returns>
        Task CancelAsync();
    }
}
