// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PropertiesComponentViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel
{
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using FluentResults;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="PropertiesComponent" />
    /// </summary>
    public class PropertiesComponentViewModel : DisposableObject, IPropertiesComponentViewModel
    {
        /// <summary>
        /// Handler registered on <see cref="ISelectionMediator.OnTreeSelectionChanged" />, retained so it can be unsubscribed on disposal
        /// </summary>
        private readonly Action<ViewerNodeViewModel> onTreeSelectionChangedHandler;

        /// <summary>
        /// Backing field for the <see cref="IsVisible" />
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Backing field for the <see cref="ParameterHaveChanges" />
        /// </summary>
        private bool parameterHaveChanges;

        /// <summary>
        /// Backing field for the <see cref="ParametersInUse" />
        /// </summary>
        private List<ParameterBase> parametersInUse = [];

        /// <summary>
        /// Backing field for the <see cref="SelectedParameter" />
        /// </summary>
        private ParameterBase selectedParameter;

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Cache of the parameter editors shown in the properties panel, keyed by <see cref="ParameterBase" />, so the
        /// editor is not rebuilt on every render (which would drop keyboard focus while the user types)
        /// </summary>
        private readonly Dictionary<ParameterBase, IDetailsComponentViewModel> panelEditorCache = new();

        /// <summary>
        /// Cache of the parameter editors shown in the submit confirmation dialog, keyed by <see cref="ParameterBase" />,
        /// so each editor keeps its state while the dialog is open
        /// </summary>
        private readonly Dictionary<ParameterBase, IDetailsComponentViewModel> dialogEditorCache = new();

        /// <summary>
        /// Creates a new instance of type <see cref="PropertiesComponentViewModel" />
        /// </summary>
        /// <param name="babylonInterop">the <see cref="IBabylonInterop" /></param>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="selectionMediator">the <see cref="ISelectionMediator" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public PropertiesComponentViewModel(IBabylonInterop babylonInterop, ISessionService sessionService, ISelectionMediator selectionMediator, ICDPMessageBus messageBus)
        {
            this.BabylonInterop = babylonInterop;
            this.SessionService = sessionService;
            this.SelectionMediator = selectionMediator;
            this.messageBus = messageBus;

            this.OnParameterValueSetChanged = new EventCallbackFactory().Create(this, async ((IValueSet,int) valueSet) => { await this.ParameterValueSetChanged(valueSet); });

            this.onTreeSelectionChangedHandler = nodeViewModel => this.OnSelectionChanged(nodeViewModel.SceneObject);
            this.SelectionMediator.OnTreeSelectionChanged += this.onTreeSelectionChangedHandler;
            this.SelectionMediator.OnModelSelectionChanged += this.OnSelectionChanged;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ISelectionMediator" /> events and releases the resources used by this view model
        /// </summary>
        /// <param name="disposing">Value asserting if this component should dispose or not</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.SelectionMediator.OnTreeSelectionChanged -= this.onTreeSelectionChangedHandler;
                this.SelectionMediator.OnModelSelectionChanged -= this.OnSelectionChanged;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets the <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> that have changed;
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> ChangedParameterValueSetRelations { get; set; } = new();

        /// <summary>
        /// Gets the original (pre-edit) <see cref="IValueSet" /> of every changed <see cref="ParameterBase" />, captured
        /// on first edit. It serves both the submit dialog's "old value" column (via <see cref="GetOriginalValue" />) and
        /// the revert, which restores the exact value set - including its switch kind, since the displayed value may come
        /// from a different switch field than <see cref="ParameterValueSetBase.Manual" />.
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> OriginalValueSets { get; } = new();

        /// <summary>
        /// Injected property to get access to <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator" />
        /// </summary>
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        public IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase" /> and <see cref="IValueSet" /> of the selected <see cref="SceneObject" />
        /// </summary>
        public Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase" /> to fill the details
        /// </summary>
        public ParameterBase SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// The list of parameters that the selected <see cref="SceneObject" /> uses
        /// </summary>
        public List<ParameterBase> ParametersInUse
        {
            get => this.parametersInUse;
            set => this.RaiseAndSetIfChanged(ref this.parametersInUse, value);
        }

        /// <summary>
        /// Gets or sets if the parameters have changes
        /// </summary>
        public bool ParameterHaveChanges
        {
            get => this.parameterHaveChanges;
            set => this.RaiseAndSetIfChanged(ref this.parameterHaveChanges, value);
        }

        /// <summary>
        /// Gets or sets if this component is visible
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Event callback for when a <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> has changed
        /// </summary>
        public EventCallback<(IValueSet,int)> OnParameterValueSetChanged { get; set; }

        /// <summary>
        /// When the button for submit changes is clicked. Persists every changed <see cref="IValueSet" /> in a single
        /// write and surfaces the outcome as a toast notification; on success the tracked changes are cleared so the
        /// changed-parameter highlighting resets.
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the write</returns>
        public async Task<Result> OnSubmit()
        {
            List<Thing> clones = [];
            Iteration iterationClone = null;

            foreach (var valueSet in this.ChangedParameterValueSetRelations.Values)
            {
                if (valueSet is not ParameterValueSetBase parameterValueSetBase)
                {
                    continue;
                }

                var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                clonedParameterValueSet.Manual = valueSet.ActualValue;
                clones.Add(clonedParameterValueSet);
                iterationClone ??= parameterValueSetBase.GetContainerOfType<Iteration>().Clone(false);
            }

            if (clones.Count > 0)
            {
                var result = await this.SessionService.CreateOrUpdateThingsWithNotification(iterationClone, clones,
                    new NotificationDescription
                    {
                        OnSuccess = "Parameter values updated successfully",
                        OnError = "Failed to update the parameter values"
                    });

                if (!result.IsSuccess)
                {
                    return result;
                }

                this.ChangedParameterValueSetRelations.Clear();
                this.OriginalValueSets.Clear();
                this.panelEditorCache.Clear();
                this.dialogEditorCache.Clear();
            }

            this.SelectionMediator.SceneObjectHasChanges = false;
            this.ParameterHaveChanges = false;
            this.SelectionMediator.RaiseOnParameterSubmitted();
            return Result.Ok();
        }

        /// <summary>
        /// Reverts an unsubmitted change on the given <see cref="ParameterBase" />, restoring its original value in the
        /// tracked relations, the editors and the scene preview.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> whose change should be discarded</param>
        public void RevertChange(ParameterBase parameter)
        {
            if (!this.ChangedParameterValueSetRelations.Remove(parameter))
            {
                return;
            }

            if (this.OriginalValueSets.Remove(parameter, out var originalValueSet))
            {
                this.ParameterValueSetRelations[parameter] = originalValueSet;
                this.SelectionMediator.SelectedSceneObjectClone?.UpdateParameter(parameter, originalValueSet);
            }

            this.panelEditorCache.Remove(parameter);
            this.dialogEditorCache.Remove(parameter);

            this.ParameterHaveChanges = this.ChangedParameterValueSetRelations.Count > 0;
            this.SelectionMediator.SceneObjectHasChanges = this.ParameterHaveChanges;
            this.SelectionMediator.RaiseOnParameterChanged();
        }

        /// <summary>
        /// Clears the dialog editor cache so the submit confirmation dialog rebuilds its editors from the current values
        /// when it is opened.
        /// </summary>
        public void OnSubmitDialogOpened()
        {
            this.dialogEditorCache.Clear();
        }

        /// <summary>
        /// Clears the editor caches when the submit confirmation dialog closes so the properties panel reflects any edits
        /// or reverts made inside the dialog.
        /// </summary>
        public void OnSubmitDialogClosed()
        {
            this.dialogEditorCache.Clear();
            this.panelEditorCache.Clear();
        }

        /// <summary>
        /// Gets the <see cref="ParameterBase" />s that have an unsubmitted change, in tracking order.
        /// </summary>
        /// <returns>The changed parameters</returns>
        public IReadOnlyList<ParameterBase> GetChangedParameters()
        {
            return this.ChangedParameterValueSetRelations.Keys.ToList();
        }

        /// <summary>
        /// Gets the display name of the given <see cref="ParameterBase" />, including its scale short name when present.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <returns>The formatted name, e.g. "Length [m]"</returns>
        public string GetParameterDisplayName(ParameterBase parameter)
        {
            return FormatParameterName(parameter);
        }

        /// <summary>
        /// Gets the original (pre-edit) value of the given <see cref="ParameterBase" /> for display in the dialog.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <returns>The formatted original value, or an empty string when it was not captured</returns>
        public string GetOriginalValue(ParameterBase parameter)
        {
            return this.OriginalValueSets.TryGetValue(parameter, out var original) ? FormatValue(original.ActualValue) : string.Empty;
        }

        /// <summary>
        /// Gets a memoized editor for the given <see cref="ParameterBase" /> to be shown inside the submit confirmation
        /// dialog, so the user can adjust the value before submitting.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> to edit</param>
        /// <returns>The <see cref="IDetailsComponentViewModel" /> for the parameter</returns>
        public IDetailsComponentViewModel GetDialogEditor(ParameterBase parameter)
        {
            var editor = this.GetOrCreateEditor(parameter, this.dialogEditorCache);
            editor.IsVisible = true;
            return editor;
        }

        /// <summary>
        /// Asserts whether the given <see cref="ParameterBase" /> has an unsubmitted change, used to highlight its label.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> to check</param>
        /// <returns>True when the parameter has a pending change</returns>
        public bool HasChanges(ParameterBase parameter)
        {
            return this.ChangedParameterValueSetRelations.ContainsKey(parameter);
        }

        /// <summary>
        /// Formats the display name of a <see cref="ParameterBase" />, appending its scale short name when present.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <returns>The formatted name, e.g. "Length [m]"</returns>
        private static string FormatParameterName(ParameterBase parameter)
        {
            var scale = parameter.Scale?.ShortName;
            return string.IsNullOrEmpty(scale) ? parameter.ParameterType?.Name : $"{parameter.ParameterType?.Name} [{scale}]";
        }

        /// <summary>
        /// Formats a <see cref="ValueArray{T}" /> for display, joining its components with a comma.
        /// </summary>
        /// <param name="value">The values to format</param>
        /// <returns>The joined value string</returns>
        private static string FormatValue(IEnumerable<string> value)
        {
            return string.Join(", ", value);
        }

        /// <summary>
        /// Gets the current used <see cref="IValueSet" />
        /// </summary>
        /// <returns>the <see cref="IValueSet" /></returns>
        public IValueSet GetUsedValueSet()
        {
            if (this.SelectedParameter is not null && this.ParameterValueSetRelations.TryGetValue(this.SelectedParameter, out var valueSet))
            {
                return valueSet;
            }

            return null;
        }

        /// <summary>
        /// Creates (or returns the memoized) <see cref="IDetailsComponentViewModel" /> for the currently selected
        /// parameter shown in the properties panel.
        /// </summary>
        /// <returns>
        /// a <see cref="IDetailsComponentViewModel" /> based on this <see cref="IPropertiesComponentViewModel" />
        /// </returns>
        public IDetailsComponentViewModel CreateDetailsComponentViewModel()
        {
            return this.GetOrCreateEditor(this.SelectedParameter, this.panelEditorCache);
        }

        /// <summary>
        /// Gets an editor for the given <paramref name="parameter" /> from the given <paramref name="cache" />, creating
        /// and memoizing it on first use so it is not rebuilt on every render (which drops keyboard focus mid-edit).
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> to edit, or null when nothing is selected</param>
        /// <param name="cache">The cache the editor is stored in</param>
        /// <returns>The <see cref="IDetailsComponentViewModel" /></returns>
        private IDetailsComponentViewModel GetOrCreateEditor(ParameterBase parameter, Dictionary<ParameterBase, IDetailsComponentViewModel> cache)
        {
            if (parameter is null)
            {
                return new DetailsComponentViewModel(false, null, null, this.OnParameterValueSetChanged, this.messageBus);
            }

            if (cache.TryGetValue(parameter, out var cachedEditor))
            {
                cachedEditor.IsVisible = this.IsVisible;
                return cachedEditor;
            }

            IValueSet valueSet;

            if (this.ChangedParameterValueSetRelations.TryGetValue(parameter, out var changedValueSet))
            {
                valueSet = changedValueSet;
            }
            else if (this.ParameterValueSetRelations.TryGetValue(parameter, out var relatedValueSet))
            {
                valueSet = relatedValueSet;
            }
            else
            {
                valueSet = null;
            }

            var callback = new EventCallbackFactory().Create(this, async ((IValueSet, int) value) => { await this.ApplyParameterChange(parameter, value.Item1); });
            var editor = new DetailsComponentViewModel(this.IsVisible, parameter.ParameterType, valueSet, callback, this.messageBus);
            cache[parameter] = editor;
            return editor;
        }

        /// <summary>
        /// Event for when a <see cref="IValueSet" /> asociated to the currently selected <see cref="ParameterBase" /> has
        /// changed. Routes to <see cref="ApplyParameterChange" /> for the selected parameter.
        /// </summary>
        /// <param name="valueTuple">The updated <see cref="IValueSet"/> with the index</param>
        /// <returns>A <see cref="Task" /></returns>
        public Task ParameterValueSetChanged((IValueSet valueSet,int _) valueTuple)
        {
            return this.ApplyParameterChange(this.SelectedParameter, valueTuple.valueSet);
        }

        /// <summary>
        /// Applies a value change to the given <paramref name="parameter" />: validates it, tracks it as a pending change
        /// (capturing the original value on first edit) and updates the scene preview.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> being edited</param>
        /// <param name="valueSet">The updated <see cref="IValueSet" /></param>
        /// <returns>A <see cref="Task" /></returns>
        public Task ApplyParameterChange(ParameterBase parameter, IValueSet valueSet)
        {
            if (parameter is null || valueSet is not ParameterValueSetBase parameterValueSetBase)
            {
                return Task.CompletedTask;
            }

            var validationMessageBuilder = new StringBuilder();
            var newValueArray = new ValueArray<string>(parameterValueSetBase.ActualValue);

            if (parameter.ParameterType is CompoundParameterType compoundParameterType)
            {
                var components = compoundParameterType.Component.ToList();

                for (var componentIndex = 0; componentIndex < components.Count; componentIndex++)
                {
                    var value = parameterValueSetBase.ActualValue[componentIndex];
                    validationMessageBuilder.Append(ParameterValueValidator.Validate(value, components[componentIndex].ParameterType, components[componentIndex]?.Scale));
                }
            }
            else
            {
                validationMessageBuilder.Append(ParameterValueValidator.Validate(parameterValueSetBase.ActualValue.First(), parameter.ParameterType, parameter.Scale));
            }

            if (!string.IsNullOrEmpty(validationMessageBuilder.ToString()))
            {
                // Do not stage an invalid value; keep the button enabled only while other valid changes are pending.
                this.ParameterHaveChanges = this.ChangedParameterValueSetRelations.Count > 0;
                return Task.CompletedTask;
            }

            this.SelectionMediator.SceneObjectHasChanges = true;
            this.ParameterHaveChanges = true;

            var clonedValueSetBase = parameterValueSetBase.Clone(false);

            var alreadyChanged = this.ChangedParameterValueSetRelations.ContainsKey(parameter);
            var hasRelation = this.ParameterValueSetRelations.TryGetValue(parameter, out var existingValueSet);

            if (!alreadyChanged && hasRelation)
            {
                this.OriginalValueSets[parameter] = existingValueSet;
            }

            clonedValueSetBase.Manual = newValueArray;
            this.ParameterValueSetRelations[parameter] = clonedValueSetBase;
            this.ChangedParameterValueSetRelations[parameter] = clonedValueSetBase;

            var selectedClone = this.SelectionMediator.SelectedSceneObjectClone;
            selectedClone?.UpdateParameter(parameter, clonedValueSetBase);

            if (parameter.ParameterType.ShortName == SceneSettings.ShapeKindShortName)
            {
                if (selectedClone?.Primitive is not null)
                {
                    selectedClone.Primitive.HasHalo = true;
                }

                var parameters = this.ParameterValueSetRelations.Keys.Where(x => x.ParameterType.ShortName != SceneSettings.ShapeKindShortName);

                foreach (var otherParameter in parameters)
                {
                    selectedClone?.UpdateParameter(otherParameter, this.ParameterValueSetRelations[otherParameter]);
                }
            }

            this.SelectionMediator.RaiseOnParameterChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the selection of a <see cref="SceneObject" /> has changed
        /// </summary>
        /// <param name="sceneObject">the changed object</param>
        private void OnSelectionChanged(SceneObject sceneObject)
        {
            this.IsVisible = sceneObject is not null;
            this.panelEditorCache.Clear();
            this.dialogEditorCache.Clear();

            if (this.SelectionMediator.SelectedSceneObjectClone?.ParametersAsociated is null)
            {
                this.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet>();
                this.ParametersInUse = [];
                this.SelectedParameter = null;
                return;
            }

            this.ParameterValueSetRelations = this.SelectionMediator.SelectedSceneObjectClone.GetParameterValueSetRelations();

            this.ParametersInUse = this.SelectionMediator.SelectedSceneObjectClone.ParametersAsociated
                .Where(x => this.ParameterValueSetRelations.ContainsKey(x))
                .OrderBy(x => x.ParameterType.ShortName)
                .ToList();

            this.SelectedParameter = this.ParametersInUse.FirstOrDefault();
        }
    }
}
