// --------------------------------------------------------------------------------------------------------
// <copyright file="IParticipantCreationViewModel.cs" company="RHEA System S.A.">
//  Copyright (c) 2022 RHEA System S.A.
// 
//  Author: Antoine Théate, Sam Gerené, Alex Vorobiev, Alexander van Delft, Martin Risseeuw
// 
//  This file is part of UI-DSM.
//  The UI-DSM web application is used to review an ECSS-E-TM-10-25 model.
// 
//  The UI-DSM application is provided to the community under the Apache License 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
	using CDP4Common.CommonData;
	using CDP4Common.EngineeringModelData;
	using CDP4Common.SiteDirectoryData;
	using Microsoft.AspNetCore.Components;

	/// <summary>
	///     Interface definition for <see cref="ElementDefinitionCreationViewModel" />
	/// </summary>
	public interface IElementDefinitionCreationViewModel
	{
		/// <summary>
		/// A collection of available <see cref="Category" />s
		/// </summary>
		IEnumerable<Category> AvailableCategories { get; set; }

		/// <summary>
		/// A collection of available <see cref="DomainOfExpertise" />s
		/// </summary>
		IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

		/// <summary>
		///     An <see cref="EventCallback" /> to invoke on form submit
		/// </summary>
		EventCallback OnValidSubmit { get; set; }

		/// <summary>
		/// Value indicating if the <see cref="ElementDefinition"/> is top element
		/// </summary>
		bool IsTopElement { get; set; }

		/// <summary>
		/// The <see cref="ElementDefinition" /> to create or edit
		/// </summary>
		ElementDefinition ElementDefinition { get; set; }

		/// <summary>
		/// Selected <see cref="Category" />
		/// </summary>
		IEnumerable<Category> SelectedCategories { get; set; }

		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// Override this method if you will perform an asynchronous operation and
		/// want the component to refresh when that operation is completed.
		/// </summary>
		void OnInitialized();
	}
}
