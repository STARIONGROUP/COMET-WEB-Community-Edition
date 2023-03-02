// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace COMETwebapp.ViewModels.Components.ReferenceData
{
    using System.Reactive.Linq;

    using CDP4Common.SiteDirectoryData;
    
    using CDP4Dal;
    using CDP4Dal.Events;

    using COMETwebapp.SessionManagement;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    
    using DevExpress.Blazor.Internal;
    
    using DynamicData;

    using ReactiveUI;

    /// <summary>
    ///     View model for the <see cref="ParameterTypeTable" /> component
    /// </summary>
    public class ParameterTypeTableViewModel : ReactiveObject, IParameterTypeTableViewModel, IDisposable
    {
        /// <summary>
        /// A collection of all <see cref="ParameterTypeRowViewModel" />
        /// </summary>
        private IEnumerable<ParameterTypeRowViewModel> allRows = new List<ParameterTypeRowViewModel>();

        /// <summary>
        /// A reactive collection of <see cref="ParameterTypeRowViewModel" />
        /// </summary>
        public SourceList<ParameterTypeRowViewModel> Rows { get; } = new();

        /// <summary>
        ///     Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<ParameterType> DataSource { get; } = new();

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        private readonly ISessionAnchor SessionAnchor;

        /// <summary>
        ///     A collection of <see cref="IDisposable" />
        /// </summary>
        private readonly List<IDisposable> disposables = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionAnchor">The <see cref="ISessionAnchor" /></param>
        public ParameterTypeTableViewModel(ISessionAnchor sessionAnchor)
        {
            this.SessionAnchor = sessionAnchor;

            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.SessionAnchor.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .Subscribe(ParameterType => this.addNewParameterType(ParameterType));
            this.disposables.Add(addListener);

            var updateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.SessionAnchor.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .Subscribe(ParameterType => this.updateParameterType(ParameterType));
            this.disposables.Add(updateListener);

            var rdlUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.SessionAnchor.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .Subscribe(this.RefreshContainerName);
            this.disposables.Add(rdlUpdateListener);
        }

        /// <summary>
        /// Refresh the displayed container name for the category rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary"/>.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            foreach (var parameter in this.Rows.Items)
            {
                if (parameter.ContainerName != rdl.ShortName)
                {
                    parameter.ContainerName = rdl.ShortName;
                }
            }
        }

        /// <summary>
        ///   Adds a new <see cref="Person" /> 
        /// </summary>
        public void addNewParameterType(ParameterType parameterType)
        {
            var newRows = new List<ParameterTypeRowViewModel>(this.allRows);
            newRows.Add(new ParameterTypeRowViewModel(parameterType));
            this.UpdateRows(newRows);
        }

        /// <summary>
        ///   Updates the <see cref="ParameterType" /> 
        /// </summary>  
        public void updateParameterType(ParameterType parameterType)
        {
            var updatedRows = new List<ParameterTypeRowViewModel>(this.allRows);
            var index = updatedRows.FindIndex(x => x.ParameterType.Iid == parameterType.Iid);
            updatedRows[index] = new ParameterTypeRowViewModel(parameterType);
            this.UpdateRows(updatedRows);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.disposables.ForEach(x => x.Dispose());
            this.disposables.Clear();
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="parameterTypes">A collection of <see cref="ParameterType" /></param>
        public void UpdateProperties(IEnumerable<ParameterType> parameterTypes)
        {
            this.allRows = parameterTypes.Select(x => new ParameterTypeRowViewModel(x));
            this.UpdateRows(this.allRows);
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on a collection of
        /// <see cref="ParameterTypeRowViewModel" /> to display.
        /// </summary>
        /// <param name="rowsToDisplay">A collection of <see cref="ParameterTypeRowViewModel" /></param>
        private void UpdateRows(IEnumerable<ParameterTypeRowViewModel> rowsToDisplay)
        {
            rowsToDisplay = rowsToDisplay.ToList();

            var deletedRows = this.Rows.Items.Where(x => rowsToDisplay.All(r => r.ParameterType.Iid != x.ParameterType.Iid)).ToList();
            var addedRows = rowsToDisplay.Where(x => this.Rows.Items.All(r => r.ParameterType.Iid != x.ParameterType.Iid)).ToList();
            var existingRows = rowsToDisplay.Where(x => this.Rows.Items.Any(r => r.ParameterType.Iid == x.ParameterType.Iid)).ToList();

            this.Rows.RemoveMany(deletedRows);
            this.Rows.AddRange(addedRows);

            foreach (var existingRow in existingRows)
            {
                this.Rows.Items.First(x => x.ParameterType.Iid == existingRow.ParameterType.Iid).UpdateProperties(existingRow);
            }
        }

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public void OnInitializedAsync()
        {
            foreach (var siteReferenceDataLibrary in this.SessionAnchor.Session.RetrieveSiteDirectory().SiteReferenceDataLibrary)
            {
                this.DataSource.AddRange(siteReferenceDataLibrary.ParameterType);
            }
            this.UpdateProperties(this.DataSource.Items);
        }
    }
}
