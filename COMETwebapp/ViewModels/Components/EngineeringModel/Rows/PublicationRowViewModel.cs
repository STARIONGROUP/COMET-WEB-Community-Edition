// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Rows
{
    using System.Globalization;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for a <see cref="Publication" />
    /// </summary>
    public class PublicationRowViewModel : BaseDataItemRowViewModel<Publication>
    {
        /// <summary>
        /// The backing field for <see cref="CreatedOn"/>
        /// </summary>
        private string createdOn;

        /// <summary>
        /// The backing field for <see cref="Domains"/>
        /// </summary>
        private string domains;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationRowViewModel" /> class.
        /// </summary>
        /// <param name="publication">The associated <see cref="Publication" /></param>
        public PublicationRowViewModel(Publication publication) : base(publication)
        {
            this.CreatedOn = publication.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss");
            this.Domains = string.Join(DomainsSeparator, publication.Domain.Select(x => x.ShortName));
        }

        /// <summary>
        /// The separator used to join the list of domains from a publication
        /// </summary>
        public const string DomainsSeparator = ", ";

        /// <summary>
        /// The creation date time value, as a <see cref="string"/>
        /// </summary>
        public string CreatedOn
        {
            get => this.createdOn;
            set => this.RaiseAndSetIfChanged(ref this.createdOn, value);
        }

        /// <summary>
        /// The domains related with the publication
        /// </summary>
        public string Domains
        {
            get => this.domains;
            set => this.RaiseAndSetIfChanged(ref this.domains, value);
        }
    }
}
