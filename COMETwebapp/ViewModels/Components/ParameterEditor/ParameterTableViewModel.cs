// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.ParameterEditor.ParameterTable" />
    /// </summary>
    public class ParameterTableViewModel : DisposableObject, IParameterTableViewModel
    {
        /// <summary>
        /// The <see cref="IPermissionService" />
        /// </summary>
        private readonly IPermissionService permissionService;

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The currently selected <see cref="ElementBase" />
        /// </summary>
        private ElementBase currentElementBase;

        /// <summary>
        /// The currently selected <see cref="Option" />
        /// </summary>
        private Option currentOption;

        /// <summary>
        /// The currently selected <see cref="ParameterType" />
        /// </summary>
        private ParameterType currentParameterType;

        /// <summary>
        /// Gets or sets the <see cref="ParameterBaseRowViewModel" /> for this <see cref="ParameterTableViewModel" />
        /// </summary>
        private DomainOfExpertise domainOfExpertise;

        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />
        /// </summary>
        private bool isOnEditMode;

        /// <summary>
        /// Creates a new instance of <see cref="ParameterTableViewModel" />
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Value asserting if only owned <see cref="ParameterOrOverrideBase" /> should be visible
        /// </summary>
        private bool ownedParameters;

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Creates a new instance of <see cref="ParameterTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public ParameterTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.messageBus = messageBus;

            this.Disposables.Add(this.messageBus.Listen<HaveComponentParameterTypeSelectedEvent>()
                .Subscribe(x => this.HandleComponentSelected(x.HaveComponentParameter)));

            this.sessionService = sessionService;
            this.permissionService = this.sessionService.Session.PermissionService;
        }

        /// <summary>
        /// Indicates if compound parameter edit popup is visible
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// The <see cref="IHaveComponentParameterTypeEditor" /> to show in the popup
        /// </summary>
        public IHaveComponentParameterTypeEditor HaveComponentParameterTypeEditorViewModel { get; set; }

        /// <summary>
        /// Gets the collection of the <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public SourceList<ParameterBaseRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Initializes this <see cref="IParameterTableViewModel" />
        /// </summary>
        /// <param name="currentIteration">The current <see cref="Iteration" /></param>
        /// <param name="currentDomain">The <see cref="DomainOfExpertise" /></param>
        /// <param name="selectedOption">The select <see cref="Option" /></param>
        public void InitializeViewModel(Iteration currentIteration, DomainOfExpertise currentDomain, Option selectedOption)
        {
            this.iteration = currentIteration;
            this.domainOfExpertise = currentDomain;
            this.currentOption = selectedOption;
            this.currentElementBase = null;
            this.currentParameterType = null;
            this.Rows.Clear();

            if (this.iteration != null)
            {
                var ownedNestedParameters = this.iteration.QueryParameterAndOverrideBases(this.currentOption, this.domainOfExpertise);
                this.Rows.AddRange(this.CreateParameterBaseRowViewModels(ownedNestedParameters, this.currentOption.Iid));
            }
        }

        /// <summary>
        /// Update the current <see cref="DomainOfExpertise" />
        /// </summary>
        /// <param name="currentDomain">The new <see cref="DomainOfExpertise" /></param>
        public void UpdateDomain(DomainOfExpertise currentDomain)
        {
            this.domainOfExpertise = currentDomain;
        }

        /// <summary>
        /// Apply filters based on <see cref="Option" />, <see cref="ElementBase" />, <see cref="ParameterType" /> and
        /// <see cref="DomainOfExpertise" />
        /// </summary>
        /// <param name="selectedOption">The selected <see cref="Option" /></param>
        /// <param name="selectedElementBase">The selected <see cref="ElementBase" /></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType" /></param>
        /// <param name="isOwnedParameters">
        /// Value asserting that the only <see cref="Thing" /> owned by the current
        /// <see cref="DomainOfExpertise" /> should be visible
        /// </param>
        public void ApplyFilters(Option selectedOption, ElementBase selectedElementBase, ParameterType selectedParameterType, bool isOwnedParameters)
        {
            if (this.iteration == null)
            {
                return;
            }

            this.currentOption = selectedOption;
            this.currentElementBase = selectedElementBase;
            this.currentParameterType = selectedParameterType;
            this.ownedParameters = isOwnedParameters;

            var rows = this.CreateRowsBasedOnFilters(this.iteration.QueryParameterAndOverrideBases(selectedOption).ToList());
            this.UpdateVisibleRows(rows);
        }

        /// <summary>
        /// Remove rows related to a <see cref="Thing" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="Thing" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            var rowsToRemove = new List<ParameterBaseRowViewModel>();

            foreach (var deletedThing in deletedThings)
            {
                switch (deletedThing)
                {
                    case ElementBase elementBase:
                        rowsToRemove.AddRange(this.Rows.Items.Where(x => x.Parameter.Container.Iid == elementBase.Iid));
                        break;
                    case ParameterOrOverrideBase parameter:
                        rowsToRemove.AddRange(this.Rows.Items.Where(x => x.Parameter.Iid == parameter.Iid));
                        break;
                    case ParameterValueSetBase parameterValueSetBase:
                        rowsToRemove.AddRange(this.Rows.Items.Where(x => x.ValueSetId == parameterValueSetBase.Iid));
                        break;
                }
            }

            this.Rows.RemoveMany(rowsToRemove);
        }

        /// <summary>
        /// Add rows related to <see cref="Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var rows = this.CreateRowsBasedOnFilters(QueryParameterOrOverrides(addedThings).ToList());

            this.Rows.AddRange(rows.Where(x => this.Rows.Items.All(r => r.ValueSetId != x.ValueSetId)));
        }

        /// <summary>
        /// Updates rows related to <see cref="Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var parameterOrOverrideBases = QueryParameterOrOverrides(updatedThings);

            foreach (var parameterValueSetBase in parameterOrOverrideBases.SelectMany(x => x.ValueSets).OfType<ParameterValueSetBase>())
            {
                var existingRow = this.Rows.Items.FirstOrDefault(x => x.ValueSetId == parameterValueSetBase.Iid);

                if (existingRow == null)
                {
                    continue;
                }

                var isReadOnly = !this.permissionService.CanWrite(parameterValueSetBase.Container);
                existingRow.UpdateProperties(isReadOnly);
            }
        }

        /// <summary>
        /// Set the <see cref="HaveComponentParameterTypeEditorViewModel" /> to show in the popup
        /// </summary>
        /// <param name="haveComponentParameterTypeEditorViewModel"></param>
        public void HandleComponentSelected(IHaveComponentParameterTypeEditor haveComponentParameterTypeEditorViewModel)
        {
            this.HaveComponentParameterTypeEditorViewModel = haveComponentParameterTypeEditorViewModel;
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Query all <see cref="ParameterOrOverrideBase" /> from the collection of <see cref="Thing" />s
        /// </summary>
        /// <param name="things">A collection of <see cref="Thing" /></param>
        /// <returns>All retrieved <see cref="ParameterOrOverrideBase" /></returns>
        private static IEnumerable<ParameterOrOverrideBase> QueryParameterOrOverrides(IEnumerable<Thing> things)
        {
            var parameters = new List<ParameterOrOverrideBase>();

            foreach (var addedThing in things)
            {
                switch (addedThing)
                {
                    case ElementBase elementBase:
                        parameters.AddRange(elementBase.QueryParameterAndOverrideBases());
                        break;
                    case ParameterOrOverrideBase parameter:
                        parameters.Add(parameter);
                        break;
                    case ParameterValueSetBase parameterValueSetBase:
                        parameters.Add(parameterValueSetBase.Container as ParameterOrOverrideBase);
                        break;
                }
            }

            return parameters.DistinctBy(x => x.Iid);
        }

        /// <summary>
        /// Create a collection of <see cref="ParameterBaseRowViewModel" /> based on selected filters
        /// </summary>
        /// <param name="parameters">
        /// The collection of <see cref="ParameterOrOverrideBase" /> for creating
        /// <see cref="ParameterBaseRowViewModel" />
        /// </param>
        /// <returns>A collection of <see cref="ParameterBaseRowViewModel" /></returns>
        private IEnumerable<ParameterBaseRowViewModel> CreateRowsBasedOnFilters(List<ParameterOrOverrideBase> parameters)
        {
            if (this.ownedParameters)
            {
                parameters.RemoveAll(x => x.Owner.Iid != this.domainOfExpertise.Iid);
            }

            if (this.currentElementBase != null)
            {
                ApplyElementBaseFilter(parameters, this.currentElementBase.Iid);
            }

            if (this.currentParameterType != null)
            {
                ApplyParameterTypeFilter(parameters, this.currentParameterType.Iid);
            }

            return this.CreateParameterBaseRowViewModels(parameters, this.currentOption.Iid).DistinctBy(x => x.ValueSetId);
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on the provided collection of <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        /// <param name="rows">The collection of <see cref="ParameterBaseRowViewModel" /> that should be displayed</param>
        private void UpdateVisibleRows(IEnumerable<ParameterBaseRowViewModel> rows)
        {
            rows = rows.ToList();

            var rowsToRemove = this.Rows.Items
                .Where(x => rows.All(p => p.ValueSetId != x.ValueSetId));

            var rowsToAdd = rows.Where(x => this.Rows.Items.All(p => p.ValueSetId != x.ValueSetId));

            this.Rows.AddRange(rowsToAdd);
            this.Rows.RemoveMany(rowsToRemove);
        }

        /// <summary>
        /// Apply a filtering base on <see cref="ParameterType" />
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /> to filter</param>
        /// <param name="parameterTypeId">The <see cref="Guid" /> of the <see cref="ParameterType" /> for filtering</param>
        private static void ApplyParameterTypeFilter(List<ParameterOrOverrideBase> parameters, Guid parameterTypeId)
        {
            parameters.RemoveAll(x => x.ParameterType.Iid != parameterTypeId);
        }

        /// <summary>
        /// Apply a filtering base on <see cref="ElementBase" />
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /> to filter</param>
        /// <param name="elementBaseId">The <see cref="Guid" /> of the <see cref="ElementBase" /> for filtering</param>
        private static void ApplyElementBaseFilter(List<ParameterOrOverrideBase> parameters, Guid elementBaseId)
        {
            var parametersToRemove = new List<Guid>();

            foreach (var parameterOrOverrideBase in parameters)
            {
                switch (parameterOrOverrideBase.Container)
                {
                    case ElementDefinition elementDefinition when elementDefinition.Iid != elementBaseId:
                    case ElementUsage elementUsage when elementUsage.Iid != elementBaseId && elementUsage.ElementDefinition.Iid != elementBaseId:
                        parametersToRemove.Add(parameterOrOverrideBase.Iid);
                        break;
                }
            }

            parameters.RemoveAll(x => parametersToRemove.Contains(x.Iid));
        }

        /// <summary>
        /// Creates <see cref="ParameterBaseRowViewModel" /> based on an <see cref="Option" />
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /></param>
        /// <param name="optionId">The <see cref="Guid" /> of the <see cref="Option" /></param>
        /// <returns>A collection of created <see cref="ParameterBaseRowViewModel" /></returns>
        private IEnumerable<ParameterBaseRowViewModel> CreateParameterBaseRowViewModels(IEnumerable<ParameterOrOverrideBase> parameters, Guid optionId)
        {
            var rows = new List<ParameterBaseRowViewModel>();

            foreach (var parameterOrOverrideBase in parameters.OrderBy(x => x.ParameterType.Name))
            {
                var isReadOnly = !this.permissionService.CanWrite(parameterOrOverrideBase);

                rows.AddRange(parameterOrOverrideBase.ValueSets.Where(x => x.ActualOption == null || x.ActualOption.Iid == optionId)
                    .Where(x => rows.TrueForAll(r => ((ParameterValueSetBase)r.ValueSet).Iid != ((ParameterValueSetBase)x).Iid))
                    .Select(x => new ParameterBaseRowViewModel(this.sessionService, isReadOnly, parameterOrOverrideBase, x, this.messageBus)));
            }

            return rows;
        }
    }
}
