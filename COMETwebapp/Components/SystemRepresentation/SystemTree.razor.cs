// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemTree.razor.cs" company="RHEA System S.A.">
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
namespace COMETwebapp.Components.SystemRepresentation
{
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using Microsoft.AspNetCore.Components;
    using Radzen.Blazor;
    using Radzen;
    using COMETwebapp.Model;
    
    public partial class SystemTree
    {
        /// <summary>
        ///     The <see cref="ISystemTreeViewModel" /> for the component
        /// </summary>
        [Parameter]
        public ISystemTreeViewModel ViewModel { get; set; }

        /// <summary>
        ///     Reference to the <see cref="RadzenDataGrid{TItem}" />
        /// </summary>
        RadzenDataGrid<SystemNode> Grid { get; set; } = new();

        /// <summary>
        ///     Supplies information to a row render
        /// </summary>
        /// <param name="row">The <see cref="RowRenderEventArgs{T}" /></param>
        void RowRender(RowRenderEventArgs<SystemNode> row)
        {
             row.Expandable = this.ViewModel.SystemNodes.Where(e => e.Title == row.Data.Title).Any();
        }

        /// <summary>
        ///     Loads children of a row
        /// </summary>
        /// <param name="parent">The <see cref="DataGridLoadChildDataEventArgs{T}" /></param>
        void LoadChildData(DataGridLoadChildDataEventArgs<SystemNode> parent)
        {
            parent.Data = this.ViewModel.SystemNodes.Where(e => e.Title == parent.Item.Title);
        }

        /// <summary>
        ///     Method invoked after each time the component has been rendered. Note that the component does
        ///     not automatically re-render after the completion of any returned <see cref="T:System.Threading.Tasks.Task" />, because
        ///     that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        ///     Set to <c>true</c> if this is the first time
        ///     <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        ///     on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        ///     The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and
        ///     <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        ///     are useful for performing interop, or interacting with values received from <c>@ref</c>.
        ///     Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        ///     once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                await Grid.ExpandRow(this.ViewModel.SystemNodes.FirstOrDefault());
            }
        }

    }
}
