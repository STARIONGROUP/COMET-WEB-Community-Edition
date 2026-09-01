// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ScaleValueDefinitionsTable.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.ReferenceData.MeasurementScales
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ScaleValueDefinitionsTable" />
    /// </summary>
    public partial class ScaleValueDefinitionsTable : DisposableComponent
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. Create and delete controls bind their enabled state to
        /// the inverse of this, so the data can still be inspected but never modified
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// The measurement scale that contains scale value definitions to display for selection
        /// </summary>
        [Parameter]
        public MeasurementScale MeasurementScale { get; set; }

        /// <summary>
        /// The method that is executed when the scale value definitions change
        /// </summary>
        [Parameter]
        public EventCallback<MeasurementScale> MeasurementScaleChanged { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a scale value definition should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// Gets or sets the scale value definition that will be handled for both edit and add forms
        /// </summary>
        private ScaleValueDefinition ScaleValueDefinition { get; set; } = new();

        /// <summary>
        /// Starts the creation flow for a new <see cref="ScaleValueDefinition" />.
        /// </summary>
        private void StartCreate()
        {
            this.ShouldCreate = true;
            this.ScaleValueDefinition = new ScaleValueDefinition { Iid = Guid.NewGuid() };
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        private void StartEdit(ScaleValueDefinitionRowViewModel row)
        {
            if (row == null)
            {
                return;
            }

            this.ShouldCreate = false;
            this.ScaleValueDefinition = row.Thing.Clone(true);
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Method that is invoked when the edit/add scale value definition form is being saved
        /// </summary>
        private void OnSaved()
        {
            if (this.ShouldCreate)
            {
                this.MeasurementScale.ValueDefinition.Add(this.ScaleValueDefinition);
            }
            else
            {
                var indexToUpdate = this.MeasurementScale.ValueDefinition.FindIndex(x => x.Iid == this.ScaleValueDefinition.Iid);
                this.MeasurementScale.ValueDefinition[indexToUpdate] = this.ScaleValueDefinition;
            }

            this.IsOnEditMode = false;
            this.MeasurementScaleChanged.InvokeAsync(this.MeasurementScale);
        }

        /// <summary>
        /// Handles cancellation of the form popup.
        /// </summary>
        private void OnCanceled()
        {
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method that is invoked when a scale value definition row is being removed
        /// </summary>
        /// <param name="row">The row to remove.</param>
        private void RemoveScaleValueDefinition(ScaleValueDefinitionRowViewModel row)
        {
            this.MeasurementScale.ValueDefinition.Remove(row.Thing);
            this.MeasurementScaleChanged.InvokeAsync(this.MeasurementScale);
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="MeasurementScale" />
        /// </summary>
        /// <returns>A collection of <see cref="ScaleValueDefinitionRowViewModel" />s to display</returns>
        private List<ScaleValueDefinitionRowViewModel> GetRows()
        {
            var rows = this.MeasurementScale.ValueDefinition?.Select(x => new ScaleValueDefinitionRowViewModel(x)).ToList();

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    row.IsAllowedToWrite = this.SessionService.Session.PermissionService.CanWrite(row.Thing);
                }
            }

            return rows;
        }
    }
}
