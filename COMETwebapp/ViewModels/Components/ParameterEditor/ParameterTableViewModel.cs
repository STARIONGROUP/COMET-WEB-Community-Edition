// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMETwebapp.Extensions;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

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
        /// Creates a new instance of <see cref="ParameterTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        public ParameterTableViewModel(ISessionService sessionService)
        {
            this.Disposables.Add(CDPMessageBus.Current.Listen<CompoundComponentSelectedEvent>()
                .Select(x => x.CompoundParameterTypeEditorViewModel)
                .Subscribe(this.HandleComponentSelected));

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
        /// The <see cref="CompoundParameterTypeEditorViewModel" /> to show in the popup
        /// </summary>
        public CompoundParameterTypeEditorViewModel CompoundParameterTypeEditorViewModel { get; set; }

        /// <summary>
        /// Set the <see cref="CompoundParameterTypeEditorViewModel" /> to show in the popup
        /// </summary>
        /// <param name="compoundParameterTypeEditorViewModel">
        /// A collection of <see cref="CompoundParameterTypeEditorViewModel" />
        /// </param>
        public void HandleComponentSelected(CompoundParameterTypeEditorViewModel compoundParameterTypeEditorViewModel)
        {
            this.CompoundParameterTypeEditorViewModel = compoundParameterTypeEditorViewModel;
            this.IsOnEditMode = true;
        }

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

            this.Rows.Clear();

            if (this.iteration != null)
            {
                var ownedNestedParameters = this.iteration.QueryParameterAndOverrideBases(selectedOption, currentDomain);
                this.Rows.AddRange(this.CreateParameterBaseRowViewModels(ownedNestedParameters, selectedOption.Iid));
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

            var parameters = (isOwnedParameters
                    ? this.iteration.QueryParameterAndOverrideBases(selectedOption, this.domainOfExpertise)
                    : this.iteration.QueryParameterAndOverrideBases(selectedOption))
                .ToList();

            if (selectedElementBase != null)
            {
                ApplyElementBaseFilter(parameters, selectedElementBase.Iid);
            }

            if (selectedParameterType != null)
            {
                ApplyParameterTypeFilter(parameters, selectedParameterType.Iid);
            }

            var rows = this.CreateParameterBaseRowViewModels(parameters, selectedOption.Iid);
            this.UpdateRows(rows);
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on the provided collection of <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        /// <param name="rows">The collection of <see cref="ParameterBaseRowViewModel" /> that should be displayed</param>
        private void UpdateRows(IEnumerable<ParameterBaseRowViewModel> rows)
        {
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
                    case ElementUsage elementUsage when elementUsage.Iid != elementBaseId || elementUsage.ElementDefinition.Iid != elementBaseId:
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
                    .Where(x => rows.All(r => ((ParameterValueSetBase)r.ValueSet).Iid != ((ParameterValueSetBase)x).Iid))
                    .Select(x => new ParameterBaseRowViewModel(this.sessionService, isReadOnly, parameterOrOverrideBase, x)));
            }

            return rows;
        }
    }
}
