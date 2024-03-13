// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.BookEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the BookEditorBody component
    /// </summary>
    public class BookEditorBodyViewModel : SingleEngineeringModelApplicationBaseViewModel, IBookEditorBodyViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedBook"/> property
        /// </summary>
        private Book selectedBook;

        /// <summary>
        /// Backing field for the <see cref="SelectedSection"/> property
        /// </summary>
        private Section selectedSection;

        /// <summary>
        /// Backing field for the <see cref="SelectedPage"/> property
        /// </summary>
        private Page selectedPage;

        /// <summary>
        /// Backing field for the <see cref="SelectedNote"/> property
        /// </summary>
        private Note selectedNote;

        /// <summary>
        /// Gets or sets the current selected <see cref="Book"/>
        /// </summary>
        public Book SelectedBook
        {
            get => this.selectedBook;
            set => this.RaiseAndSetIfChanged(ref this.selectedBook, value);
        }

        /// <summary>
        /// Gets or sets the current selected <see cref="Section"/>
        /// </summary>
        public Section SelectedSection
        {
            get => this.selectedSection;
            set => this.RaiseAndSetIfChanged(ref this.selectedSection, value);
        }

        /// <summary>
        /// Gets or sets the current selected <see cref="Page"/>
        /// </summary>
        public Page SelectedPage
        {
            get => this.selectedPage;
            set => this.RaiseAndSetIfChanged(ref this.selectedPage, value);
        }

        /// <summary>
        /// Gets or sets the current selected <see cref="Note"/>
        /// </summary>
        public Note SelectedNote
        {
            get => this.selectedNote;
            set => this.RaiseAndSetIfChanged(ref this.selectedNote, value);
        }

        /// <summary>
        /// Gets or sets the collection of available <see cref="Book"/> for this <see cref="EngineeringModel"/>
        /// </summary>
        public SourceList<Book> AvailableBooks { get; set; } = new();

        /// <summary>
        /// Gets or sets the available categories
        /// </summary>
        public List<Category> AvailableCategories { get; set; } = new();

        /// <summary>
        /// Gets or sets the active <see cref="DomainOfExpertise"/>
        /// </summary>
        public List<DomainOfExpertise> ActiveDomains { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the thing to be edited
        /// </summary>
        public Thing ThingToEdit { get; set; }

        /// <summary>
        /// Gets or sets the thing to be created
        /// </summary>
        public Thing ThingToCreate { get; set; }

        /// <summary>
        /// Gets or sets the thing to be deleted
        /// </summary>
        public Thing ThingToDelete { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEditorPopupViewModel"/>
        /// </summary>
        public IEditorPopupViewModel EditorPopupViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConfirmCancelPopupViewModel"/>
        /// </summary>
        public IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookEditorBodyViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public BookEditorBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedBook).Subscribe(_ => this.OnSelectedBookChanged()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedSection).Subscribe(_ => this.OnSelectedSectionChanged()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedPage).Subscribe(_ => this.OnSelectedPageChanged()));

            this.EditorPopupViewModel = new EditorPopupViewModel();

            this.ConfirmCancelPopupViewModel = new ConfirmCancelPopupViewModel
            {
                ContentText = "Are you sure you want to delete the thing?",
            };

            this.ConfirmCancelPopupViewModel.OnCancel = new EventCallbackFactory().Create(this, () => this.ConfirmCancelPopupViewModel.IsVisible = false);

            this.ConfirmCancelPopupViewModel.OnConfirm = new EventCallbackFactory().Create(this, async () =>
            {
                await this.OnDeleteThing();
                this.ConfirmCancelPopupViewModel.IsVisible = false;
            });
        }

        /// <summary>
        /// Handler for when the selected book changed
        /// </summary>
        private void OnSelectedBookChanged()
        {
            this.SelectedSection = null;
        }

        /// <summary>
        /// Handler for when the selected section changed
        /// </summary>
        private void OnSelectedSectionChanged()
        {
            this.SelectedPage = null;
        }

        /// <summary>
        /// Handler for when the selected page changed
        /// </summary>
        private void OnSelectedPageChanged()
        {
            this.SelectedNote = null;
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            return this.OnThingChanged();
        }

        /// <summary>
        /// Update this view model properties when the <see cref="EngineeringModel" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnThingChanged()
        {
            this.IsLoading = true;

            this.AvailableBooks.Edit(inner =>
            {
                inner.Clear();
                inner.AddRange(this.CurrentThing.Book);
            });

            this.ActiveDomains.Clear();
            this.ActiveDomains.AddRange(this.CurrentThing.EngineeringModelSetup.ActiveDomain);

            this.AvailableCategories.Clear();
            var categories = this.CurrentThing.RequiredRdls.SelectMany(x => x.DefinedCategory);
            this.AvailableCategories.AddRange(categories);

            this.EditorPopupViewModel.AvailableCategories = this.AvailableCategories;
            this.EditorPopupViewModel.ActiveDomains = this.ActiveDomains;

            this.IsLoading = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates that the thing is a valid thing for the operations in this ViewModel
        /// </summary>
        /// <param name="thing">the thing to validate</param>
        private static void ValidateThing(Thing thing)
        {
            if (thing is not Book && thing is not Section && thing is not Page && thing is not Note)
            {
                throw new ArgumentException("The thing to should be a (Book, Section, Page or Note)", nameof(thing));
            }
        }

        /// <summary>
        /// Sets the thing to be created
        /// </summary>
        /// <param name="thing">the thing</param>
        public void SetThingToCreate(Thing thing)
        {
            ValidateThing(thing);

            this.ThingToCreate = thing;

            var header = "";

            switch (this.ThingToCreate)
            {
                case Book: 
                    header = "Create a new Book"; 
                    break;
                case Section: 
                    header = "Create a new Section";
                    break;
                case Page: 
                    header = "Create a new Page";
                    break;
                case Note: 
                    header = "Create a new Note";
                    break;
            }

            this.EditorPopupViewModel.HeaderText = header;
            this.EditorPopupViewModel.Item = this.ThingToCreate;
            this.EditorPopupViewModel.OnConfirmClick = new EventCallbackFactory().Create(this, this.OnCreateThing);
            this.EditorPopupViewModel.OnCancelClick = new EventCallbackFactory().Create(this, () => this.EditorPopupViewModel.IsVisible = false);
            this.EditorPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Hanlder for when the user request to create a new thing (Book,Section,Page or Note)
        /// </summary>
        /// <returns></returns>
        public async Task OnCreateThing()
        {
            if (this.ThingToCreate == null)
            {
                throw new InvalidOperationException("The thing to create can't be null");
            }

            ValidateThing(this.ThingToCreate);

            Thing thingContainer;

            switch (this.ThingToCreate)
            {
                case Book: 
                    thingContainer = this.CurrentThing;
                    break;
                case Section: 
                    thingContainer = this.SelectedBook;
                    break;
                case Page: 
                    thingContainer = this.SelectedSection;
                    break;
                case Note: 
                    thingContainer = this.SelectedPage;
                    break;

                default:
                    this.ThingToCreate = null;
                    return;
            }

            await this.SessionService.CreateThing(thingContainer.Clone(false), this.ThingToCreate);

            this.ThingToCreate = null;
            this.EditorPopupViewModel.IsVisible = false;
        }

        /// <summary>
        /// Sets the thing to be edited
        /// </summary>
        /// <param name="thing">the thing</param>
        public void SetThingToEdit(Thing thing)
        {
            ValidateThing(thing);

            this.ThingToEdit = thing;

            var header = "";

            switch (this.ThingToEdit)
            {
                case Book:
                    header = "Edit the Book";
                    break;
                case Section:
                    header = "Edit the Section";
                    break;
                case Page:
                    header = "Edit the Page";
                    break;
                case Note:
                    header = "Edit the Note";
                    break;
            }

            this.EditorPopupViewModel.HeaderText = header;
            this.EditorPopupViewModel.Item = this.ThingToEdit;
            this.EditorPopupViewModel.OnConfirmClick = new EventCallbackFactory().Create(this, this.OnEditThing);
            this.EditorPopupViewModel.OnCancelClick = new EventCallbackFactory().Create(this, () => this.EditorPopupViewModel.IsVisible = false);
            this.EditorPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Handler for when the user request to edit a thing (Book,Section,Page or Note)
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public async Task OnEditThing()
        {
            if (this.ThingToEdit == null)
            {
                throw new InvalidOperationException("The thing to edit can't be null");
            }

            ValidateThing(this.ThingToEdit);

            var thingContainer = this.ThingToEdit.Container;
            var thingContainerClone = thingContainer.Clone(false);

            await this.SessionService.UpdateThing(thingContainerClone, this.ThingToEdit.Clone(false));

            this.ThingToEdit = null;
            this.EditorPopupViewModel.IsVisible = false;
        }

        /// <summary>
        /// Sets the thing to be deleted
        /// </summary>
        /// <param name="thingToDelete">the thing</param>
        public void SetThingToDelete(Thing thingToDelete)
        {
            ValidateThing(thingToDelete);

            this.ThingToDelete = thingToDelete;

            this.ConfirmCancelPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Hanlder for when the user request to delete a thing (Book,Section,Page or Note)
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public async Task OnDeleteThing()
        {
            if (this.ThingToDelete == null)
            {
                throw new InvalidOperationException("The thing to delete can't be null");
            }

            ValidateThing(this.ThingToDelete);

            var thingContainer = this.ThingToDelete.Container;
            var thingContainerClone = thingContainer.Clone(false);

            await this.SessionService.DeleteThing(thingContainerClone, this.ThingToDelete.Clone(false));
            
            this.ThingToDelete = null;
        }
    }
}

