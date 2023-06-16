// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerNode.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Components.Viewer
{
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Viewer;
    
    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the ViewerNode component
    /// </summary>
    public partial class ViewerNode
    {
        /// <summary>
        /// Gets or sets the <see cref="IBaseNodeViewModel"/>
        /// </summary>
        [Parameter]
        public ViewerNodeViewModel ViewModel { get; set; }

        /// <summary>
        /// Level of the tree. Increases by one for each nested element
        /// </summary>
        [Parameter]
        public int Level { get; set; }
        
        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.ViewModel.Level = this.Level;

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsDrawn,
                              x => x.ViewModel.IsExpanded,
                              x => x.ViewModel.IsSelected,
                              x => x.ViewModel.IsSceneObjectVisible)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Sets parameters supplied by the component's parent in the render tree.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A <see cref="Task"/> that completes when the component has finished updating and rendering itself.</returns>
        /// <remarks>
        /// <para>
        /// Parameters are passed when <see cref="SetParametersAsync(ParameterView)"/> is called. It is not required that
        /// the caller supply a parameter value for all of the parameters that are logically understood by the component.
        /// </para>
        /// <para>
        /// The default implementation of <see cref="SetParametersAsync(ParameterView)"/> will set the value of each property
        /// decorated with <see cref="ParameterAttribute" /> or <see cref="CascadingParameterAttribute" /> that has
        /// a corresponding value in the <see cref="ParameterView" />. Parameters that do not have a corresponding value
        /// will be unchanged.
        /// </para>
        /// </remarks>
        public override Task SetParametersAsync(ParameterView parameters)
        {
            foreach (var parameter in parameters)
            {
                switch (parameter.Name)
                {
                    case nameof(this.ViewModel):
                        this.ViewModel = parameter.Value as ViewerNodeViewModel;
                        break;
                    case nameof(this.Level):
                        this.Level = (int)parameter.Value;
                        break;
                }
            }

            return base.SetParametersAsync(ParameterView.Empty);
        }
    }
}
