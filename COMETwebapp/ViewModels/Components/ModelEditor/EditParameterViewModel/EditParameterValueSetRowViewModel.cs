// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterValueSetRowViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// One editable row of the values grid: a scalar parameter's single value, or a single component of a
    /// <see cref="CompoundParameterType" /> (each component has its own parameter type and scale, as in the desktop
    /// IME). The Manual and Reference values are independently editable; Actual and Computed are read-only. The switch
    /// and the staged clone are owned by the parent <see cref="EditParameterValueSetGroupViewModel" /> and shared with
    /// the value set's other component rows.
    /// </summary>
    public class EditParameterValueSetRowViewModel : DisposableObject
    {
        /// <summary>
        /// The parent group owning the shared switch and staged clone for this value set.
        /// </summary>
        private readonly EditParameterValueSetGroupViewModel group;

        /// <summary>
        /// Backing field for <see cref="ActualValue" />.
        /// </summary>
        private string actualValue;

        /// <summary>
        /// Backing field for <see cref="ComputedValue" />.
        /// </summary>
        private string computedValue;

        /// <summary>
        /// Creates a new instance of the <see cref="EditParameterValueSetRowViewModel" /> class.
        /// </summary>
        /// <param name="group">The parent <see cref="EditParameterValueSetGroupViewModel" />.</param>
        /// <param name="componentIndex">The value-array index this row edits (0 for a scalar parameter).</param>
        /// <param name="componentType">The <see cref="ParameterType" /> of the component (or the scalar parameter type).</param>
        /// <param name="name">The component short name (empty for a scalar parameter).</param>
        /// <param name="scaleShortName">The component scale short name (empty when none).</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /> passed to the value editors.</param>
        public EditParameterValueSetRowViewModel(EditParameterValueSetGroupViewModel group, int componentIndex, ParameterType componentType, string name, string scaleShortName, ICDPMessageBus messageBus)
        {
            this.group = group;
            this.ComponentIndex = componentIndex;
            this.Name = name;
            this.ParameterTypeName = componentType.Name;
            this.ScaleShortName = scaleShortName;
            this.OptionName = group.OriginalValueSet.ActualOption?.Name ?? string.Empty;
            this.StateName = group.OriginalValueSet.ActualState?.Name ?? string.Empty;

            var callbackFactory = new EventCallbackFactory();

            this.ManualEditorViewModel = new ParameterTypeEditorSelectorViewModel(componentType, group.OriginalValueSet, false, messageBus, componentIndex)
            {
                ParameterValueChanged = callbackFactory.Create<(IValueSet, int)>(this, this.OnManualChanged)
            };

            this.ManualEditorViewModel.UpdateSwitchKind(ParameterSwitchKind.MANUAL);

            this.ReferenceEditorViewModel = new ParameterTypeEditorSelectorViewModel(componentType, group.OriginalValueSet, false, messageBus, componentIndex)
            {
                ParameterValueChanged = callbackFactory.Create<(IValueSet, int)>(this, this.OnReferenceChanged)
            };

            this.ReferenceEditorViewModel.UpdateSwitchKind(ParameterSwitchKind.REFERENCE);

            this.RefreshReadOnlyValues();
        }

        /// <summary>
        /// Gets the value-array index this row edits.
        /// </summary>
        public int ComponentIndex { get; }

        /// <summary>
        /// Gets the component short name (empty for a scalar parameter).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of this row's parameter type (the component's type for a compound parameter).
        /// </summary>
        public string ParameterTypeName { get; }

        /// <summary>
        /// Gets the component scale short name (empty when none).
        /// </summary>
        public string ScaleShortName { get; }

        /// <summary>
        /// Gets the name of the <see cref="Option" /> this value set applies to, or an empty string.
        /// </summary>
        public string OptionName { get; }

        /// <summary>
        /// Gets the name of the <see cref="ActualFiniteState" /> this value set applies to, or an empty string.
        /// </summary>
        public string StateName { get; }

        /// <summary>
        /// Gets the read-only resolved actual value for this component, reflecting any staged edits.
        /// </summary>
        public string ActualValue
        {
            get => this.actualValue;
            private set => this.RaiseAndSetIfChanged(ref this.actualValue, value);
        }

        /// <summary>
        /// Gets the read-only computed value for this component.
        /// </summary>
        public string ComputedValue
        {
            get => this.computedValue;
            private set => this.RaiseAndSetIfChanged(ref this.computedValue, value);
        }

        /// <summary>
        /// Gets the editor for this component's Manual value (always editable).
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ManualEditorViewModel { get; }

        /// <summary>
        /// Gets the editor for this component's Reference value (always editable).
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ReferenceEditorViewModel { get; }

        /// <summary>
        /// Gets the switch selector shared with the value set's other component rows.
        /// </summary>
        public IParameterSwitchKindSelectorViewModel ParameterSwitchKindSelectorViewModel => this.group.ParameterSwitchKindSelectorViewModel;

        /// <summary>
        /// Recomputes the read-only Actual Value / Computed columns for this component from the current staged (or
        /// original) value set.
        /// </summary>
        public void RefreshReadOnlyValues()
        {
            var valueSet = this.group.Current;
            this.ActualValue = ParameterValueFormatter.ValueAt(valueSet.ActualValue, this.ComponentIndex);
            this.ComputedValue = ParameterValueFormatter.ValueAt(valueSet.Computed, this.ComponentIndex);
        }

        /// <summary>
        /// Handles a Manual edit emitted by the editor: merges this component's edited value onto the shared clone.
        /// </summary>
        /// <param name="value">The emitted value set clone and value-array index.</param>
        private void OnManualChanged((IValueSet valueSet, int _) value)
        {
            if (value.valueSet is not ParameterValueSetBase emitted)
            {
                return;
            }

            this.group.ApplyManual(this.ComponentIndex, emitted.Manual);
            this.RefreshReadOnlyValues();
        }

        /// <summary>
        /// Handles a Reference edit emitted by the editor: merges this component's edited value onto the shared clone.
        /// </summary>
        /// <param name="value">The emitted value set clone and value-array index.</param>
        private void OnReferenceChanged((IValueSet valueSet, int _) value)
        {
            if (value.valueSet is not ParameterValueSetBase emitted)
            {
                return;
            }

            this.group.ApplyReference(this.ComponentIndex, emitted.Reference);
            this.RefreshReadOnlyValues();
        }

    }
}
