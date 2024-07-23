// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SortableList.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
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

namespace COMETwebapp.Components.Shared
{
    using System.Diagnostics.CodeAnalysis;

    using COMET.Web.Common.Components;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    /// <summary>
    /// Class used to support the sortable list razor component
    /// </summary>
    public partial class SortableList<T> : DisposableComponent
    {
        /// <summary>
        /// The self <see cref="DotNetObjectReference" />
        /// </summary>
        private DotNetObjectReference<SortableList<T>> selfReference;

        /// <summary>
        /// The injected <see cref="IJSRuntime" />
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// The sortable item template to be displayed in the listing
        /// </summary>
        [Parameter]
        public RenderFragment<T> SortableItemTemplate { get; set; }

        /// <summary>
        /// The list of items to be displayed
        /// </summary>
        [Parameter]
        [AllowNull]
        public List<T> Items { get; set; }

        /// <summary>
        /// Callback for when an item is updated in the list
        /// </summary>
        [Parameter]
        public EventCallback<(int oldIndex, int newIndex)> OnUpdate { get; set; }

        /// <summary>
        /// Callback for when an item is removed from the list
        /// </summary>
        [Parameter]
        public EventCallback<(int oldIndex, int newIndex)> OnRemove { get; set; }

        /// <summary>
        /// The unique identifier for the list
        /// </summary>
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The group where the list belongs
        /// </summary>
        [Parameter]
        public string Group { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The pull parameter data
        /// </summary>
        [Parameter]
        public string Pull { get; set; }

        /// <summary>
        /// The condition to check if the put option should be available
        /// </summary>
        [Parameter]
        public bool Put { get; set; } = true;

        /// <summary>
        /// The sort parameter for list sorting
        /// </summary>
        [Parameter]
        public bool Sort { get; set; } = true;

        /// <summary>
        /// The handle for list events
        /// </summary>
        [Parameter]
        public string Handle { get; set; } = string.Empty;

        /// <summary>
        /// The filter for the list data
        /// </summary>
        [Parameter]
        public string Filter { get; set; }

        /// <summary>
        /// Condition to check if a fallback should be forced
        /// </summary>
        [Parameter]
        public bool ForceFallback { get; set; } = true;

        /// <summary>
        /// Gets or sets the custom css class to be applied in the container component
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Method used to update an item from the list, invoking the OnUpdate event and passing the old and new indexes
        /// </summary>
        /// <param name="oldIndex">The old item index</param>
        /// <param name="newIndex">The new item index</param>
        [JSInvokable]
        public void OnUpdateJS(int oldIndex, int newIndex)
        {
            this.OnUpdate.InvokeAsync((oldIndex, newIndex));
        }

        /// <summary>
        /// Method used to remove an item from the list
        /// </summary>
        /// <param name="oldIndex">The old item index</param>
        /// <param name="newIndex">The new item index</param>
        [JSInvokable]
        public void OnRemoveJS(int oldIndex, int newIndex)
        {
            this.OnRemove.InvokeAsync((oldIndex, newIndex));
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered interactively and the UI has finished
        /// updating (for example, after elements have been added to the browser DOM). Any
        /// <see cref="T:Microsoft.AspNetCore.Components.ElementReference" />
        /// fields will be populated by the time this runs.
        /// This method is not invoked during prerendering or server-side rendering, because those processes
        /// are not attached to any live browser DOM and are already complete before the DOM is updated.
        /// Note that the component does not automatically re-render after the completion of any returned
        /// <see cref="T:System.Threading.Tasks.Task" />,
        /// because that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time
        /// <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and
        /// <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.selfReference = DotNetObjectReference.Create(this);
                this.Disposables.Add(this.selfReference);
                var module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Shared/SortableList.razor.js");
                await module.InvokeAsync<string>("init", this.Id, this.Group, this.Pull, this.Put, this.Sort, this.Handle, this.Filter, this.selfReference, this.ForceFallback);
            }
        }
    }
}
