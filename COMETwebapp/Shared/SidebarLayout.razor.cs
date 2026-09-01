// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SidebarLayout.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Shared
{
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    /// <summary>
    /// Class used to support the <see cref="SidebarLayout" /> component
    /// </summary>
    public partial class SidebarLayout
    {
        /// <summary>
        /// The <see cref="ISessionMenuViewModel" />
        /// </summary>
        [Inject]
        public ISessionMenuViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="IJSRuntime" /> used to wire up the shell's keyboard-accessibility helpers
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Wires up the global keyboard-navigation helpers (landmark hotkeys and side bar arrow navigation) once the
        /// layout has first rendered. The helper attaches a single document-level listener and is a no-op on later calls.
        /// </summary>
        /// <param name="firstRender">A value indicating whether this is the first render of the component</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await this.InvokeKeyboardHelperAsync("cometKeyboard.init");
            }
        }

        /// <summary>
        /// Moves keyboard focus to a shell landmark by invoking the named helper. A skip link keeps its fragment
        /// <c>href</c> as a no-JavaScript fallback, but Blazor intercepts same-document navigation and only scrolls the
        /// target into view without focusing it, so the actual focus move is driven from here.
        /// </summary>
        /// <param name="focusFunction">The <c>cometKeyboard</c> focus helper to invoke</param>
        /// <returns>A <see cref="Task" /></returns>
        private Task MoveFocusTo(string focusFunction)
        {
            return this.InvokeKeyboardHelperAsync(focusFunction);
        }

        /// <summary>
        /// Invokes a <c>cometKeyboard</c> JavaScript helper, swallowing the failures that occur when the helper script
        /// is not available. Keyboard navigation is a progressive enhancement, so a missing script (which happens on a
        /// hard reload before the script has loaded) or a disconnecting circuit must never terminate the circuit
        /// </summary>
        /// <param name="functionName">The fully qualified name of the <c>cometKeyboard</c> helper to invoke</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task InvokeKeyboardHelperAsync(string functionName)
        {
            try
            {
                await this.JsRuntime.InvokeVoidAsync(functionName);
            }
            catch (JSException)
            {
                // The cometKeyboard script is not loaded yet (typically a hard reload); keyboard navigation is a
                // progressive enhancement, so this is safe to ignore.
            }
            catch (JSDisconnectedException)
            {
                // The circuit is disconnecting, so no interop can run; nothing to do.
            }
        }
    }
}
