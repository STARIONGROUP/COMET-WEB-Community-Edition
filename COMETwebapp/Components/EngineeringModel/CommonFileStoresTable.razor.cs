// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoresTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.EngineeringModel
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="CommonFileStoresTable"/>
    /// </summary>
    public partial class CommonFileStoresTable : SelectedDataItemBase<CommonFileStore, CommonFileStoreRowViewModel>
    {
        /// <summary>
        /// The <see cref="ICommonFileStoreTableViewModel" /> for this component
        /// </summary>
        [Parameter, Required]
        public ICommonFileStoreTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the value to check if the folder file structure component is visible
        /// </summary>
        public bool IsFolderFileStructureVisible { get; private set; }

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
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            base.CustomizeEditThing(e);

            var dataItem = (CommonFileStoreRowViewModel)e.DataItem;
            this.ViewModel.Thing = dataItem == null ? new CommonFileStore() : dataItem.Thing.Clone(true);
            e.EditModel = this.ViewModel.Thing;
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(CommonFileStoreRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ViewModel.Thing = row.Thing.Clone(true);
        }

        /// <summary>
        /// Method invoked when the deletion popup is confirmed
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task OnConfirmDelete()
        {
            await this.ViewModel.OnConfirmPopupButtonClick();
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method invoked when the see folder file structure button is clicked
        /// </summary>
        private void OnSeeFolderFileStructureClick()
        {
            this.ViewModel.LoadFileStructure();
            this.IsFolderFileStructureVisible = true;
        }
    }
}
