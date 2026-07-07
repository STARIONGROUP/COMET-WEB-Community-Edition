// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IEditParameterSubscriptionViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel
{
    using CDP4Common.EngineeringModelData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Interface for the <see cref="EditParameterSubscriptionViewModel" /> that drives the Edit Parameter
    /// Subscription dialog. Opened from a parameter card when the current domain has a subscription on a parameter it
    /// does not own; edits the current domain's <see cref="ParameterSubscription" /> (its Manual values and switches).
    /// </summary>
    public interface IEditParameterSubscriptionViewModel
    {
        /// <summary>
        /// Gets the <see cref="ParameterSubscription" /> being edited (a populated-state guard; not mutated).
        /// </summary>
        ParameterSubscription Subscription { get; }

        /// <summary>
        /// Gets the display label of the subscribed parameter (its type name).
        /// </summary>
        string SubscribedParameterName { get; }

        /// <summary>
        /// Gets the short name of the subscription owner (the current domain).
        /// </summary>
        string OwnerShortName { get; }

        /// <summary>
        /// Gets the model code of the subscribed parameter.
        /// </summary>
        string ModelCode { get; }

        /// <summary>
        /// Gets the editable subscription value-set rows.
        /// </summary>
        IReadOnlyList<EditParameterSubscriptionValueSetRowViewModel> ValueRows { get; }

        /// <summary>
        /// Gets or sets the callback invoked once the dialog is done (after a save or a cancel).
        /// </summary>
        EventCallback OnSubscriptionEdited { get; set; }

        /// <summary>
        /// Initializes the view model for the supplied <see cref="ParameterSubscription" />.
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> to edit.</param>
        /// <param name="iteration">The <see cref="Iteration" /> the subscription belongs to.</param>
        void SetSubscription(ParameterSubscription subscription, Iteration iteration);

        /// <summary>
        /// Commits the pending Manual / switch edits to the session and closes the dialog.
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
