// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixBody.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RelationshipMatrix
{
    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="RelationshipMatrixBody" /> component.
    /// </summary>
    public partial class RelationshipMatrixBody
    {
        /// <summary>
        /// Handles the post-assignment flow of the <see cref="COMET.Web.Common.Components.Applications.ApplicationBase{TViewModel}.ViewModel" /> property.
        /// Only the loading state is observed here; the matrix content is owned by <see cref="MatrixGrid" />, so a
        /// matrix rebuild never re-renders the configuration panel and its open multi-select dropdowns.
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the URL.
        /// The Relationship Matrix does not yet take any URL parameters.
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }
    }
}
