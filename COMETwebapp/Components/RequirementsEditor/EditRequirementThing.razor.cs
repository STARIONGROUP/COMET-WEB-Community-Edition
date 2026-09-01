// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditRequirementThing.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RequirementsEditor
{
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Tabbed form that creates or edits a <see cref="CDP4Common.EngineeringModelData.Requirement" />, a
    /// <see cref="CDP4Common.EngineeringModelData.RequirementsGroup" /> or a
    /// <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" />.
    /// </summary>
    public partial class EditRequirementThing
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a
        /// session opened from an ECSS-E-TM-10-25 Annex C3 archive. The form still renders so its data can be
        /// inspected, but the Save button is withdrawn
        /// </summary>
        private bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Gets a value indicating whether the active user may write the thing this form edits. The nested simple
        /// parameter values are parts of that one thing and are saved atomically with it, so they bind their create and
        /// delete controls to this rather than evaluating a permission per row
        /// </summary>
        /// <remarks>
        /// Answers true when creating, and when the permission cannot be determined, so the form is never blocked by an
        /// unexpected null <see cref="ISessionService.Session" /> or thing
        /// </remarks>
        private bool IsAllowedToWriteCurrentThing
        {
            get
            {
                var requirement = this.ViewModel?.RequirementThing;

                // A requirement being created is not in its container yet. Its create permission was already asserted by
                // the add button, and asking CanWrite about a container-less thing would recurse into a null container.
                if (requirement?.Container == null)
                {
                    return true;
                }

                return this.SessionService?.Session?.PermissionService?.CanWrite(requirement) ?? true;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEditRequirementThingViewModel" /> driving the form.
        /// </summary>
        [Parameter]
        public IEditRequirementThingViewModel ViewModel { get; set; }
    }
}
