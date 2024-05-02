// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasComponent.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Components.Viewer
{
    using COMETwebapp.ViewModels.Components.Viewer;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="Canvas3D"/>
    /// </summary>
    public partial class Canvas3D
    {
        /// <summary>
        /// Gets or sets the <see cref="ICanvasViewModel"/>
        /// </summary>
        [Parameter]
        public ICanvasViewModel ViewModel { get; set; }

        /// <summary>
        /// Tells if the mouse if pressed or not in the canvas component
        /// </summary>
        public bool IsMouseDown { get; private set; } = false;

        /// <summary>
        /// Tells is the scene is being moved, rotated...
        /// </summary>
        public bool IsMovingScene { get; private set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.ViewModel.InitializeViewModel();

            this.Disposables.Add(this.WhenAnyValue(x=>x.ViewModel.IsOnChangePrimitiveMode)
                .Subscribe(_=>this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Canvas on mouse down event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseDown(MouseEventArgs e)
        {
            this.IsMouseDown = true;
        }

        /// <summary>
        /// Canvas on mouse up event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public async void OnMouseUp(MouseEventArgs e)
        {
            if (!this.IsMovingScene)
            {
                await this.ViewModel.HandleMouseUp();
            }

            this.IsMouseDown = false;
        }

        /// <summary>
        /// Canvas on mouse move event
        /// </summary>
        /// <param name="e">the mouse args of the event</param>
        public void OnMouseMove(MouseEventArgs e)
        {
            this.IsMovingScene = this.IsMouseDown;
        }
    }
}
