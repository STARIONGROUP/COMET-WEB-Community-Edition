// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.BookEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the BookEditorBody component
    /// </summary>
    public class BookEditorBodyViewModel : SingleIterationApplicationBaseViewModel, IBookEditorBodyViewModel
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
        /// Gets or sets the collection of available <see cref="Book"/> for this <see cref="EngineeringModel"/>
        /// </summary>
        public SourceList<Book> AvailableBooks { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BookEditorBodyViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public BookEditorBodyViewModel(ISessionService sessionService) : base(sessionService)
        {
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedBook).Subscribe(_ => this.OnSelectedBookChanged()));
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedSection).Subscribe(_ => this.OnSelectedSectionChanged()));
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            return this.OnIterationChanged();
        }

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnIterationChanged()
        {
            this.IsLoading = true;
            await base.OnIterationChanged();
            var a = this.CurrentIteration;

            this.AvailableBooks.Edit(inner =>
            {
                inner.Clear();
                inner.AddRange(((EngineeringModel)this.CurrentIteration.Container).Book);
            });

            this.CreateFakeData();
            this.IsLoading = false;
        }

        /// <summary>
        /// Handler for when the selected book changed
        /// </summary>
        private void OnSelectedBookChanged()
        {
            this.SelectedSection = null;
            this.SelectedPage = null;
        }

        /// <summary>
        /// Handler for when the selected section changed
        /// </summary>
        private void OnSelectedSectionChanged()
        {
            this.SelectedPage = null;
        }

        private void CreateFakeData()
        {
            var book = new Book
            {
                Name = "Example Book",
                Section =
                {
                    new Section
                    {
                        Name = "Example Section",
                        Page =
                        {
                            new Page
                            {
                                Name = "Example Page",
                                Note =
                                {
                                    new BinaryNote
                                    {
                                        Name = "Binary Note"
                                    },
                                    new TextualNote
                                    {
                                        Name = "Textual Note"
                                    }
                                }
                            },
                            new Page
                            {
                                Name = "Example Page 2"
                            }
                        }
                    },
                    new Section
                    {
                        Name = "Empty Section"
                    }
                }
            };

            this.AvailableBooks.Add(book);
        }
    }
}

