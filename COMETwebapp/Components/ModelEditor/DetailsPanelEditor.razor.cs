// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsPanelEditor.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
namespace COMETwebapp.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

	using Microsoft.AspNetCore.Components;

	using ReactiveUI;

	/// <summary>
	///     Partial class for the component <see cref="ElementDefinitionDetails"/>
	/// </summary>
	public partial class DetailsPanelEditor
	{
		/// <summary>
		///     The <see cref="IElementDefinitionDetailsViewModel" /> for the component
		/// </summary>
		[Parameter]
		public IElementDefinitionDetailsViewModel ViewModel { get; set; }

		/// <summary>
		///     Optional callback invoked when the user clicks the per-card delete affordance for a
		///     <see cref="Parameter" />. When unset, the delete affordance is not rendered, so consumers
		///     other than the Model Editor remain read-only.
		/// </summary>
		[Parameter]
		public EventCallback<Parameter> OnDeleteParameter { get; set; }

		/// <summary>
		///     Optional callback invoked when the user clicks the per-card subscribe affordance for a
		///     <see cref="Parameter" /> not owned by the currently logged-in
		///     <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" /> and not yet subscribed to by it.
		///     When unset, the subscribe affordance is not rendered.
		/// </summary>
		[Parameter]
		public EventCallback<Parameter> OnCreateSubscription { get; set; }

		/// <summary>
		///     Optional callback invoked when the user clicks the per-card unsubscribe affordance to remove
		///     an existing <see cref="ParameterSubscription" /> belonging to the currently logged-in
		///     <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />. When unset, the unsubscribe
		///     affordance is not rendered.
		/// </summary>
		[Parameter]
		public EventCallback<ParameterSubscription> OnDeleteSubscription { get; set; }

		/// <summary>
		///     Optional callback invoked when the user clicks the Edit affordance on the summary card.
		///     When unset, the Edit button is not rendered.
		/// </summary>
		[Parameter]
		public EventCallback OnEditElement { get; set; }

		/// <summary>
		///     Optional callback invoked when the user clicks the Delete affordance on the summary card.
		///     When unset, the Delete button is not rendered.
		/// </summary>
		[Parameter]
		public EventCallback OnDeleteElement { get; set; }

		/// <summary>
		///     Disables the Delete button on the summary card. Set by the parent when the selected element
		///     is the iteration's <see cref="Iteration.TopElement" /> — which the application forbids from
		///     being deleted.
		/// </summary>
		[Parameter]
		public bool DisableDelete { get; set; }

		/// <summary>
		///     Method invoked when the component is ready to start, having received its
		///     initial parameters from its parent in the render tree.
		///     Override this method if you will perform an asynchronous operation and
		///     want the component to refresh when that operation is completed.
		/// </summary>
		/// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
		protected override Task OnInitializedAsync()
		{
			this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedSystemNode)
				.Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

			return base.OnInitializedAsync();
		}
	}
}
