// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model for the <see cref="Components.ModelEditor.EditParameter" /> dialog. Edits an existing
    /// <see cref="Parameter" /> or <see cref="ParameterOverride" />: its Basic metadata (owner, scale, group,
    /// option/state dependence, expects-override), its per-type values, and shows a read-only list of its
    /// subscriptions. All edits are staged on clones and committed as a single operation on
    /// <see cref="SaveAsync" />, or discarded on <see cref="CancelAsync" />.
    /// </summary>
    public class EditParameterViewModel : DisposableObject, IEditParameterViewModel
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
        /// The original, unmodified <see cref="ParameterOrOverrideBase" /> being edited.
        /// </summary>
        private ParameterOrOverrideBase original;

        /// <summary>
        /// The container of the edited thing — an <see cref="ElementDefinition" /> for a <see cref="Parameter" />
        /// or an <see cref="ElementUsage" /> for a <see cref="ParameterOverride" />.
        /// </summary>
        private Thing container;

        /// <summary>
        /// The value of <see cref="Parameter.IsOptionDependent" /> captured when the dialog opened, used to detect
        /// whether the option dependence was toggled.
        /// </summary>
        private bool originalIsOptionDependent;

        /// <summary>
        /// The Iid of <see cref="Parameter.StateDependence" /> captured when the dialog opened, used to detect
        /// whether the state dependence changed.
        /// </summary>
        private Guid? originalStateDependenceIid;

        /// <summary>
        /// The Iid of the parameter's owner captured when the dialog opened, used to detect an owner change.
        /// </summary>
        private Guid? originalOwnerIid;

        /// <summary>
        /// Backing field for <see cref="ValuesEditable" />.
        /// </summary>
        private bool valuesEditable = true;

        /// <summary>
        /// Backing field for <see cref="IsOnComponentEditMode" />.
        /// </summary>
        private bool isOnComponentEditMode;

        /// <summary>
        /// The per-value-set groups backing <see cref="ValueRows" />; kept to collect their pending clones on save
        /// and to dispose them when the dialog is re-opened.
        /// </summary>
        private List<EditParameterValueSetGroupViewModel> valueSetGroups = [];

        /// <summary>
        /// Creates a new instance of the <see cref="EditParameterViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" />.</param>
        public EditParameterViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;
            this.messageBus = messageBus;

            var callbackFactory = new EventCallbackFactory();

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = callbackFactory.Create<DomainOfExpertise>(this, selectedOwner =>
                {
                    if (this.Parameter is not null)
                    {
                        this.Parameter.Owner = selectedOwner;
                        this.UpdateValuesEditable();
                    }
                })
            };

            this.MeasurementScaleSelectorViewModel = new MeasurementScaleSelectorViewModel(sessionService)
            {
                OnSelectedMeasurementScaleChange = callbackFactory.Create<MeasurementScale>(this, selectedScale =>
                {
                    if (this.ParameterAsParameter is not null)
                    {
                        this.ParameterAsParameter.Scale = selectedScale;
                    }
                })
            };

            // A compound/sampled value editor (e.g. the SampledFunctionParameterType table) opens its own popup by
            // publishing this event on the bus; hold onto the editor VM and show it in the component-edit popup.
            this.Disposables.Add(this.messageBus.Listen<HaveComponentParameterTypeSelectedEvent>()
                .Subscribe(x =>
                {
                    this.HaveComponentParameterTypeEditorViewModel = x.HaveComponentParameter;
                    this.IsOnComponentEditMode = true;
                }));
        }

        /// <summary>
        /// Gets the working clone of the <see cref="ParameterOrOverrideBase" /> being edited.
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the edited thing is a <see cref="Parameter" />.
        /// </summary>
        public bool IsParameter => this.Parameter is Parameter;

        /// <summary>
        /// Gets the working clone typed as a <see cref="Parameter" />, or <c>null</c> when a
        /// <see cref="ParameterOverride" /> is being edited.
        /// </summary>
        public Parameter ParameterAsParameter => this.Parameter as Parameter;

        /// <summary>
        /// Gets the display name of the edited parameter type.
        /// </summary>
        public string ParameterTypeName => this.Parameter?.ParameterType?.Name ?? string.Empty;

        /// <summary>
        /// Gets a value indicating whether the value sets may be edited. Turns <c>false</c> once the user changes the
        /// option or state dependence, because the server regenerates the value sets on save — any edits made against
        /// the current (soon-to-be-replaced) value sets would be discarded, so editing is blocked to avoid confusion.
        /// </summary>
        public bool ValuesEditable
        {
            get => this.valuesEditable;
            private set => this.RaiseAndSetIfChanged(ref this.valuesEditable, value);
        }

        /// <summary>
        /// Gets the message explaining why the values grid is temporarily disabled, or an empty string when it is
        /// editable. Differs for an option/state dependence change (server regenerates the value sets) versus an
        /// owner change (the value sets are being handed to another domain).
        /// </summary>
        public string ValuesDisabledReason
        {
            get
            {
                if (this.DependenceChanged())
                {
                    return "The option/state dependence changed. The value sets will be regenerated when you save; edit their values afterwards.";
                }

                if (this.OwnerChanged())
                {
                    return "The owner changed. Save the ownership change first, then edit the values as the new owner.";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component-value edit popup (used by the
        /// <see cref="SampledFunctionParameterType" /> / <see cref="CompoundParameterType" /> table editor) is visible.
        /// </summary>
        public bool IsOnComponentEditMode
        {
            get => this.isOnComponentEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnComponentEditMode, value);
        }

        /// <summary>
        /// Gets the component value editor to show in the <see cref="IsOnComponentEditMode" /> popup, as published on
        /// the bus by the editor's "Edit" button.
        /// </summary>
        public IHaveComponentParameterTypeEditor HaveComponentParameterTypeEditorViewModel { get; private set; }

        /// <summary>
        /// Gets or sets whether the edited <see cref="Parameter" /> is option-dependent. Wraps the model property so
        /// that toggling it re-evaluates <see cref="ValuesEditable" />.
        /// </summary>
        public bool IsOptionDependent
        {
            get => this.ParameterAsParameter?.IsOptionDependent ?? false;
            set
            {
                if (this.ParameterAsParameter is not null)
                {
                    this.ParameterAsParameter.IsOptionDependent = value;
                    this.UpdateValuesEditable();
                }
            }
        }

        /// <summary>
        /// Gets or sets the edited <see cref="Parameter" />'s state dependence. Wraps the model property so that
        /// changing it re-evaluates <see cref="ValuesEditable" />.
        /// </summary>
        public ActualFiniteStateList SelectedStateDependence
        {
            get => this.ParameterAsParameter?.StateDependence;
            set
            {
                if (this.ParameterAsParameter is not null)
                {
                    this.ParameterAsParameter.StateDependence = value;
                    this.UpdateValuesEditable();
                }
            }
        }

        /// <summary>
        /// Gets the model code of the edited parameter.
        /// </summary>
        public string ModelCode => this.original?.ModelCode() ?? string.Empty;

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" /> driving the Owner selector.
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IMeasurementScaleSelectorViewModel" /> driving the Scale selector.
        /// </summary>
        public IMeasurementScaleSelectorViewModel MeasurementScaleSelectorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="ActualFiniteStateList" />s selectable for the State Dependence field.
        /// </summary>
        public IEnumerable<ActualFiniteStateList> PossibleFiniteStates { get; private set; } = [];

        /// <summary>
        /// Gets the <see cref="ParameterGroup" />s selectable for the Group field.
        /// </summary>
        public IEnumerable<ParameterGroup> ParameterGroups { get; private set; } = [];

        /// <summary>
        /// Gets the editable value-set rows shown on the Values tab.
        /// </summary>
        public IReadOnlyList<EditParameterValueSetRowViewModel> ValueRows { get; private set; } = [];

        /// <summary>
        /// Gets the read-only subscription rows shown on the Subscriptions tab.
        /// </summary>
        public IReadOnlyList<ParameterSubscriptionInfoRowViewModel> SubscriptionRows { get; private set; } = [];

        /// <summary>
        /// Gets or sets the callback invoked once the dialog is done (after a save or a cancel).
        /// </summary>
        public EventCallback OnParameterEdited { get; set; }

        /// <summary>
        /// Initializes the view model for the supplied <see cref="ParameterOrOverrideBase" />.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> to edit.</param>
        /// <param name="iteration">The <see cref="Iteration" /> the parameter belongs to.</param>
        /// <param name="currentDomain">The currently logged-in <see cref="DomainOfExpertise" />.</param>
        public void SetParameter(ParameterOrOverrideBase parameter, Iteration iteration, DomainOfExpertise currentDomain)
        {
            if (parameter is null)
            {
                return;
            }

            foreach (var previousGroup in this.valueSetGroups)
            {
                previousGroup.Dispose();
            }

            this.original = parameter;
            this.container = parameter.Container;
            this.Parameter = (ParameterOrOverrideBase)parameter.Clone(false);

            var parameterType = parameter.ParameterType;

            this.originalIsOptionDependent = (parameter as Parameter)?.IsOptionDependent ?? false;
            this.originalStateDependenceIid = (parameter as Parameter)?.StateDependence?.Iid;
            this.originalOwnerIid = parameter.Owner?.Iid;
            this.ValuesEditable = true;

            this.PossibleFiniteStates = iteration.ActualFiniteStateList;
            this.ParameterGroups = this.container is ElementDefinition elementDefinition ? elementDefinition.ParameterGroup : [];

            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise = parameter
                .GetContainerOfType<EngineeringModel>()
                .EngineeringModelSetup.ActiveDomain
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(false, parameter.Owner);

            if (parameterType is QuantityKind quantityKind)
            {
                this.MeasurementScaleSelectorViewModel.AvailableMeasurementScales = quantityKind.AllPossibleScale.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
                this.MeasurementScaleSelectorViewModel.SelectedMeasurementScale = this.ParameterAsParameter?.Scale;
            }

            IEnumerable<ParameterValueSetBase> valueSets = parameter switch
            {
                Parameter p => p.ValueSet.Cast<ParameterValueSetBase>(),
                ParameterOverride po => po.ValueSet.Cast<ParameterValueSetBase>(),
                _ => []
            };

            this.valueSetGroups = valueSets
                .OrderBy(vs => vs.ActualOption?.Name, StringComparer.InvariantCultureIgnoreCase)
                .ThenBy(vs => vs.ActualState?.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(vs => new EditParameterValueSetGroupViewModel(parameterType, vs, this.messageBus))
                .ToList();

            this.ValueRows = this.valueSetGroups.SelectMany(valueSetGroup => valueSetGroup.Rows).ToList();

            this.SubscriptionRows = parameter.ParameterSubscription
                .SelectMany(subscription => subscription.ValueSet.Select(vs => new ParameterSubscriptionInfoRowViewModel(
                    subscription.Owner?.ShortName ?? string.Empty,
                    vs.ActualOption?.Name ?? string.Empty,
                    vs.ActualState?.Name ?? string.Empty,
                    vs.ValueSwitch.ToString(),
                    ParameterValueFormatter.Format(vs.ActualValue))))
                .ToList();
        }

        /// <summary>
        /// Commits the pending metadata and value edits to the session and closes the dialog. When the parameter's
        /// option or state dependence was toggled the value sets are omitted from the batch: the CDP4-COMET server
        /// regenerates them from the new dependence configuration (the same way it creates them for a new parameter).
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous save operation.</returns>
        public async Task SaveAsync()
        {
            if (this.Parameter is null || this.container is null)
            {
                await this.OnParameterEdited.InvokeAsync();
                return;
            }

            try
            {
                var clonedContainer = this.container.Clone(false);
                List<Thing> thingsToUpdate = [this.Parameter];

                if (!this.DependenceChanged())
                {
                    // ponytail: only send value sets when the option/state dependence is unchanged; otherwise the
                    // existing value sets no longer match and the server regenerates them from the parameter alone.
                    thingsToUpdate.AddRange(this.valueSetGroups
                        .Select(valueSetGroup => valueSetGroup.GetPendingValueSet())
                        .Where(pending => pending is not null));
                }

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedContainer, thingsToUpdate, this.GetNotificationDescription());
            }
            finally
            {
                await this.OnParameterEdited.InvokeAsync();
            }
        }

        /// <summary>
        /// Discards the pending edits and closes the dialog.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous cancel operation.</returns>
        public Task CancelAsync()
        {
            return this.OnParameterEdited.InvokeAsync();
        }

        /// <summary>
        /// Re-evaluates <see cref="ValuesEditable" /> after an option/state dependence or owner change. Editing the
        /// values is blocked once the owner changes because the value sets are being handed to a different domain — the
        /// current values should be edited by the new owner after the ownership change is saved.
        /// </summary>
        private void UpdateValuesEditable()
        {
            this.ValuesEditable = !this.DependenceChanged() && !this.OwnerChanged();
        }

        /// <summary>
        /// Determines whether the parameter's owner changed relative to the value captured when the dialog opened.
        /// </summary>
        /// <returns><c>true</c> when the owner changed.</returns>
        private bool OwnerChanged()
        {
            return this.Parameter?.Owner?.Iid != this.originalOwnerIid;
        }

        /// <summary>
        /// Determines whether the option or state dependence of the edited parameter changed relative to the values
        /// captured when the dialog opened. Always <c>false</c> for a <see cref="ParameterOverride" />.
        /// </summary>
        /// <returns><c>true</c> when the dependence configuration changed.</returns>
        private bool DependenceChanged()
        {
            if (this.ParameterAsParameter is not { } editedParameter)
            {
                return false;
            }

            return editedParameter.IsOptionDependent != this.originalIsOptionDependent
                   || editedParameter.StateDependence?.Iid != this.originalStateDependenceIid;
        }

        /// <summary>
        /// Builds the <see cref="NotificationDescription" /> surfaced by the write pipeline.
        /// </summary>
        /// <returns>The <see cref="NotificationDescription" />.</returns>
        private NotificationDescription GetNotificationDescription()
        {
            var kind = this.IsParameter ? "Parameter" : "Parameter Override";

            return new NotificationDescription
            {
                OnSuccess = $"{kind} '{this.ParameterTypeName}' updated",
                OnError = $"Failed to update {kind} '{this.ParameterTypeName}'"
            };
        }

    }
}
