﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementTable.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.SiteDirectory.UserManagement
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;
    using COMETwebapp.ViewModels.Components.SiteDirectory.UserManagement;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="UserManagementTable" />
    /// </summary>
    public partial class UserManagementTable : SelectedDeprecatableDataItemBase<Person, PersonRowViewModel>
    {
        /// <summary>
        /// The <see cref="IUserManagementTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IUserManagementTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when a custom summary calculation is required, allowing you to
        /// perform custom calculations based on the data displayed in the grid.
        /// </summary>
        /// <param name="e">A <see cref="GridCustomSummaryEventArgs" /></param>
        public static void CustomSummary(GridCustomSummaryEventArgs e)
        {
            switch (e.SummaryStage)
            {
                case GridCustomSummaryStage.Start:
                    e.TotalValue = 0;
                    break;
                case GridCustomSummaryStage.Calculate:
                    if (e.DataItem is PersonRowViewModel { IsActive: false })
                    {
                        e.TotalValue = (int)e.TotalValue + 1;
                    }

                    break;
                case GridCustomSummaryStage.Finalize:
                default:
                    break;
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(PersonRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ShouldCreateThing = false;
            this.ViewModel.SelectPerson(row.Thing.Clone(true));
            this.ViewModel.Thing = row.Thing.Clone(true);
        }

        /// <summary>
        /// Method invoked before creating a new thing
        /// </summary>
        private void OnAddThingClick()
        {
            this.ShouldCreateThing = true;
            this.IsOnEditMode = true;
            this.ViewModel.SelectPerson(new Person() { IsActive = true });
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Method invoked when the summary text of a summary item is being displayed, allowing you to customize
        /// the text as needed. Override this method to modify the summary text based on specific conditions.
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeSummaryDisplayTextEventArgs" /></param>
        private static void CustomizeSummaryDisplayText(GridCustomizeSummaryDisplayTextEventArgs e)
        {
            if (e.Item.Name == "Inactive")
            {
                e.DisplayText = $"{e.Value} Inactive";
            }
        }
    }
}