// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IBookEditorBodyViewModel.cs" company="RHEA System S.A.">
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

    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using DynamicData;

    /// <summary>
    /// ViewModel for the BookEditorBody component
    /// </summary>
    public interface IBookEditorBodyViewModel : ISingleEngineeringModelApplicationBaseViewModel
    {
        /// <summary>
        /// Gets or sets the current selected <see cref="Book"/>
        /// </summary>
        Book SelectedBook { get; set; }

        /// <summary>
        /// Gets or sets the current selected <see cref="Section"/>
        /// </summary>
        Section SelectedSection { get; set; }

        /// <summary>
        /// Gets or sets the current selected <see cref="Page"/>
        /// </summary>
        Page SelectedPage { get; set; }

        /// <summary>
        /// Gets or sets the current selected <see cref="Note"/>
        /// </summary>
        Note SelectedNote { get; set; }

        /// <summary>
        /// Gets or sets the collection of available <see cref="Book"/> for this <see cref="EngineeringModel"/>
        /// </summary>
        SourceList<Book> AvailableBooks { get; set; }

        /// <summary>
        /// Gets or sets the available categories
        /// </summary>
        List<Category> AvailableCategories { get; set; }

        /// <summary>
        /// Gets or sets the active <see cref="DomainOfExpertise"/>
        /// </summary>
        List<DomainOfExpertise> ActiveDomains { get; set; }

        /// <summary>
        /// Gets or sets the thing to be edited
        /// </summary>
        Thing ThingToEdit { get; set; }

        /// <summary>
        /// Gets or sets the thing to be created
        /// </summary>
        Thing ThingToCreate { get; set; }

        /// <summary>
        /// Gets or sets the thing to be deleted
        /// </summary>
        Thing ThingToDelete { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEditorPopupViewModel"/>
        /// </summary>
        IEditorPopupViewModel EditorPopupViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConfirmCancelPopupViewModel"/>
        /// </summary>
        IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; set; }

        /// <summary>
        /// Sets the thing to be created
        /// </summary>
        /// <param name="thing">the thing</param>
        void SetThingToCreate(Thing thing);

        /// <summary>
        /// Hanlder for when the user request to create a new thing (Book,Section,Page or Note)
        /// </summary>
        /// <returns></returns>
        Task OnCreateThing();

        /// <summary>
        /// Sets the thing to be deleted
        /// </summary>
        /// <param name="thingToDelete">the thing</param>
        void SetThingToDelete(Thing thingToDelete);

        /// <summary>
        /// Hanlder for when the user request to delete a thing (Book,Section,Page or Note)
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        Task OnDeleteThing();

        /// <summary>
        /// Sets the thing to be edited
        /// </summary>
        /// <param name="thing">the thing</param>
        void SetThingToEdit(Thing thing);

        /// <summary>
        /// Handler for when the user request to edit a thing (Book,Section,Page or Note)
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        Task OnEditThing();
    }
}
