// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterCard.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Common
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///     Partial class for the <see cref="ParameterCard" /> component. Renders a single parameter card
    ///     for an <see cref="ElementDefinitionDetailsRowViewModel" />, exposing affordances for subscribe,
    ///     unsubscribe, override, delete-override, delete-parameter, and move-to-group operations. The card
    ///     is draggable so it can be dropped onto a parameter-group section header to reassign group membership.
    /// </summary>
    public partial class ParameterCard
    {
        /// <summary>
        ///     Tracks whether the per-state value breakdown is currently expanded. Toggled by
        ///     <see cref="ToggleStatesExpanded" />.
        /// </summary>
        private bool statesExpanded;

        /// <summary>
        ///     Toggles <see cref="statesExpanded" /> to show or hide the per-state value breakdown for
        ///     state-dependent parameters.
        /// </summary>
        private void ToggleStatesExpanded()
        {
            this.statesExpanded = !this.statesExpanded;
        }

        /// <summary>
        ///     Gets or sets the <see cref="ElementDefinitionDetailsRowViewModel" /> whose data this card renders.
        /// </summary>
        [Parameter]
        public ElementDefinitionDetailsRowViewModel Row { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the edit affordance for the row's
        ///     <see cref="ParameterOrOverrideBase" /> — the <see cref="ParameterOverride" /> when one exists on the
        ///     selected usage, otherwise the underlying <see cref="Parameter" />. When unset, the edit affordance is
        ///     not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterOrOverrideBase> OnEditParameter { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the delete affordance for a <see cref="Parameter" />.
        ///     When unset, the delete affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<Parameter> OnDeleteParameter { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the subscribe affordance for a
        ///     <see cref="Parameter" /> not yet subscribed to by the current domain.
        ///     When unset, the subscribe affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<Parameter> OnCreateSubscription { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the unsubscribe affordance to remove an existing
        ///     <see cref="ParameterSubscription" />. When unset, the unsubscribe affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterSubscription> OnDeleteSubscription { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the create-override affordance. The tuple carries
        ///     the source <see cref="Parameter" /> and the host <see cref="ElementUsage" />.
        ///     When unset, the create-override affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<(Parameter Parameter, ElementUsage HostUsage)> OnCreateOverride { get; set; }

        /// <summary>
        ///     Optional callback invoked when the user clicks the delete-override affordance to remove an
        ///     existing <see cref="ParameterOverride" />. When unset, the delete-override affordance is not rendered.
        /// </summary>
        [Parameter]
        public EventCallback<ParameterOverride> OnDeleteOverride { get; set; }

        /// <summary>
        ///     Callback raised when the user starts dragging this card. Carries the row being dragged so the
        ///     host component can track which card is in flight.
        /// </summary>
        [Parameter]
        public EventCallback<ElementDefinitionDetailsRowViewModel> OnCardDragStart { get; set; }

        /// <summary>
        ///     Callback raised when the drag operation for this card ends (either by a successful drop or by
        ///     the user cancelling). The host component should reset its drag-tracking state on this event.
        /// </summary>
        [Parameter]
        public EventCallback<ElementDefinitionDetailsRowViewModel> OnCardDragEnd { get; set; }
    }
}
