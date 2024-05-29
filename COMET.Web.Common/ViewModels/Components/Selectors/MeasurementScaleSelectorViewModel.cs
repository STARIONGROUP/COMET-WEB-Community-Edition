// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScaleSelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables the user to select a <see cref="MeasurementScale" />
    /// </summary>
    public class MeasurementScaleSelectorViewModel : DisposableObject, IMeasurementScaleSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedMeasurementScale" />
        /// </summary>
        private MeasurementScale selectedDomainOfExpertise;

        /// <summary>
        /// Creates a new instance of <see cref="DomainOfExpertiseSelectorViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public MeasurementScaleSelectorViewModel(ISessionService sessionService)
        {
            this.AvailableMeasurementScales = sessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.Scale).OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedMeasurementScale).SubscribeAsync(this.OnSelectedMeasurementScaleChange.InvokeAsync));
        }

        /// <summary>
        /// Gets or sets the callback that is executed when the <see cref="SelectedMeasurementScale" /> property has changed
        /// </summary>
        public EventCallback<MeasurementScale> OnSelectedMeasurementScaleChange { get; set; }

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        public IEnumerable<MeasurementScale> AvailableMeasurementScales { get; set; }

        /// <summary>
        /// The currently selected <see cref="MeasurementScale" />
        /// </summary>
        public MeasurementScale SelectedMeasurementScale
        {
            get => this.selectedDomainOfExpertise;
            set => this.RaiseAndSetIfChanged(ref this.selectedDomainOfExpertise, value);
        }
    }
}
