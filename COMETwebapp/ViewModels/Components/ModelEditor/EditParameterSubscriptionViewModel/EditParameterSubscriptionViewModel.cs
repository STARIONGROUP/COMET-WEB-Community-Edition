// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterSubscriptionViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model for the <see cref="Components.ModelEditor.EditParameterSubscription" /> dialog. Mirrors the desktop
    /// IME's "Edit Parameter Subscription" dialog: the subscribed parameter, owner and model code are read-only, and
    /// the current domain's subscription value sets (Manual + switch) are edited. All edits are staged on clones and
    /// committed on <see cref="SaveAsync" /> or discarded on <see cref="CancelAsync" />.
    /// </summary>
    public class EditParameterSubscriptionViewModel : DisposableObject, IEditParameterSubscriptionViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to perform the write.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="ICDPMessageBus" /> passed to the value editors.
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The <see cref="Iteration" /> used as the transaction top container.
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The subscription value sets paired with the group editing a throwaway proxy of each — kept to collect
        /// pending edits on save and to dispose the groups when the dialog re-opens.
        /// </summary>
        private List<(ParameterSubscriptionValueSet Original, EditParameterValueSetGroupViewModel Group)> valueSetGroups = [];

        /// <summary>
        /// Creates a new instance of the <see cref="EditParameterSubscriptionViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" />.</param>
        public EditParameterSubscriptionViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;
            this.messageBus = messageBus;
        }

        /// <summary>
        /// Gets the <see cref="ParameterSubscription" /> being edited (a populated-state guard; not mutated).
        /// </summary>
        public ParameterSubscription Subscription { get; private set; }

        /// <summary>
        /// Gets the display label of the subscribed parameter (its type name).
        /// </summary>
        public string SubscribedParameterName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the short name of the subscription owner (the current domain).
        /// </summary>
        public string OwnerShortName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the model code of the subscribed parameter.
        /// </summary>
        public string ModelCode { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the editable subscription value-set rows.
        /// </summary>
        public IReadOnlyList<EditParameterSubscriptionValueSetRowViewModel> ValueRows { get; private set; } = [];

        /// <summary>
        /// Gets or sets the callback invoked once the dialog is done (after a save or a cancel).
        /// </summary>
        public EventCallback OnSubscriptionEdited { get; set; }

        /// <summary>
        /// Initializes the view model for the supplied <see cref="ParameterSubscription" />.
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> to edit.</param>
        /// <param name="iteration">The <see cref="Iteration" /> the subscription belongs to.</param>
        public void SetSubscription(ParameterSubscription subscription, Iteration iteration)
        {
            if (subscription is null || subscription.Container is not ParameterOrOverrideBase subscribedParameter)
            {
                return;
            }

            this.iteration = iteration;

            this.Subscription = subscription;

            var parameterType = subscribedParameter.ParameterType;
            var owningElementShortName = (subscribedParameter.Container as ElementBase)?.ShortName;

            this.SubscribedParameterName = string.IsNullOrWhiteSpace(owningElementShortName)
                ? parameterType?.Name ?? string.Empty
                : $"{parameterType?.Name} ({owningElementShortName})";

            this.OwnerShortName = subscription.Owner?.ShortName ?? string.Empty;
            this.ModelCode = subscription.ModelCode();

            foreach (var previousGroup in this.valueSetGroups)
            {
                previousGroup.Group.Dispose();
            }

            this.valueSetGroups = subscription.ValueSet
                .OrderBy(vs => vs.ActualOption?.Name, StringComparer.InvariantCultureIgnoreCase)
                .ThenBy(vs => vs.ActualState?.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(vs => (vs, new EditParameterValueSetGroupViewModel(parameterType, CreateProxy(vs), this.messageBus)))
                .ToList();

            this.ValueRows = this.valueSetGroups
                .SelectMany(pair => pair.Group.Rows.Select(componentRow => new EditParameterSubscriptionValueSetRowViewModel(
                    componentRow,
                    pair.Original.Owner?.ShortName ?? string.Empty,
                    ParameterValueFormatter.ValueAt(pair.Original.Reference, componentRow.ComponentIndex),
                    pair.Original.ActualOption?.Name ?? string.Empty,
                    pair.Original.ActualState?.Name ?? string.Empty)))
                .ToList();
        }

        /// <summary>
        /// Builds a throwaway <see cref="ParameterValueSet" /> mirroring the subscription value set's arrays so the
        /// shared per-type editors can edit the Manual value; the edited Manual is copied back on save.
        /// </summary>
        /// <param name="valueSet">The subscription value set to mirror.</param>
        /// <returns>The proxy <see cref="ParameterValueSet" />.</returns>
        private static ParameterValueSet CreateProxy(ParameterSubscriptionValueSet valueSet)
        {
            return new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(valueSet.Manual),
                Computed = new ValueArray<string>(valueSet.Computed),
                Reference = new ValueArray<string>(valueSet.Reference),
                Formula = new ValueArray<string>(valueSet.Formula),
                Published = new ValueArray<string>(valueSet.Computed),
                ValueSwitch = valueSet.ValueSwitch
            };
        }

        /// <summary>
        /// Commits the pending Manual / switch edits to the session and closes the dialog.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous save operation.</returns>
        public async Task SaveAsync()
        {
            if (this.Subscription is null || this.iteration is null)
            {
                await this.OnSubscriptionEdited.InvokeAsync();
                return;
            }

            try
            {
                var pendingValueSets = new List<Thing>();

                foreach (var pair in this.valueSetGroups)
                {
                    var pending = pair.Group.GetPendingValueSet();

                    if (pending is null)
                    {
                        continue;
                    }

                    var clone = (ParameterSubscriptionValueSet)pair.Original.Clone(false);
                    clone.Manual = pending.Manual;
                    clone.ValueSwitch = pending.ValueSwitch;
                    pendingValueSets.Add(clone);
                }

                if (pendingValueSets.Count > 0)
                {
                    await this.sessionService.CreateOrUpdateThingsWithNotification(this.iteration.Clone(false), pendingValueSets, this.GetNotificationDescription());
                }
            }
            finally
            {
                await this.OnSubscriptionEdited.InvokeAsync();
            }
        }

        /// <summary>
        /// Discards the pending edits and closes the dialog.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous cancel operation.</returns>
        public Task CancelAsync()
        {
            return this.OnSubscriptionEdited.InvokeAsync();
        }

        /// <summary>
        /// Builds the <see cref="NotificationDescription" /> surfaced by the write pipeline.
        /// </summary>
        /// <returns>The <see cref="NotificationDescription" />.</returns>
        private NotificationDescription GetNotificationDescription()
        {
            return new NotificationDescription
            {
                OnSuccess = $"Subscription to '{this.SubscribedParameterName}' updated",
                OnError = $"Failed to update the subscription to '{this.SubscribedParameterName}'"
            };
        }
    }
}
