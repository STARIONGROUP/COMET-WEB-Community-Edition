// --------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantCreationViewModel.cs" company="RHEA System S.A.">
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

	using COMET.Web.Common.Services.SessionManagement;
	
	using Microsoft.AspNetCore.Components;

	/// <summary>
	///     View model for the <see cref="ParticipantCreation" /> component
	/// </summary>
	public class ElementDefinitionCreationViewModel : IElementDefinitionCreationViewModel
	{
		/// <summary>
		/// The <see cref="ISessionService" />
		/// </summary>
		private readonly ISessionService sessionService;

		/// <summary>
		///     Initializes a new instance of the <see cref="ParticipantCreationViewModel" /> class.
		/// </summary>
		/// <param name="sessionService">the <see cref="ISessionService" /></param>
		public ElementDefinitionCreationViewModel(ISessionService sessionService)
		{
			this.sessionService = sessionService;
		}

		/// <summary>
		/// A collection of available <see cref="Category" />s
		/// </summary>
		public IEnumerable<Category> AvailableCategories { get; set; } = new List<Category>();

		/// <summary>
		/// A collection of available <see cref="DomainOfExpertise" />s
		/// </summary>
		public IEnumerable<DomainOfExpertise> AvailableDomains { get; set; } = new List<DomainOfExpertise>();

		/// <summary>
		/// Selected <see cref="Category" />
		/// </summary>
		public IEnumerable<Category> SelectedCategories { get; set; } = new List<Category>();

		/// <summary>
		///     An <see cref="EventCallback" /> to invoke on form submit
		/// </summary>
		public EventCallback OnValidSubmit { get; set; }

		/// <summary>
		/// Value indicating if the <see cref="ElementDefinition"/> is top element
		/// </summary>
		public bool IsTopElement { get; set; }

		/// <summary>
		/// The <see cref="ElementDefinition" /> to create or edit
		/// </summary>
		public ElementDefinition ElementDefinition { get; set; } = new();

		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// Override this method if you will perform an asynchronous operation and
		/// want the component to refresh when that operation is completed.
		/// </summary>
		public void OnInitialized()
		{
			foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
			{
				this.AvailableCategories = this.AvailableCategories.Concat(referenceDataLibrary.DefinedCategory).Where(category => category.PermissibleClass.Contains(ClassKind.ElementDefinition)).ToList();
			}

			this.AvailableDomains = this.sessionService.Session.RetrieveSiteDirectory().Domain;
		}
	}
}
