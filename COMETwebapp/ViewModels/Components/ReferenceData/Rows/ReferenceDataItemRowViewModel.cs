// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Rows
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.Extensions;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="Category" />
    /// </summary>
    public class ReferenceDataItemRowViewModel<T> : ReactiveObject where T : DefinedThing, IDeprecatableThing
    {
        /// <summary>
        /// Backing field for <see cref="ContainerName" />
        /// </summary>
        private string containerName;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated" />
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName" />
        /// </summary>
        private string shortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRowViewModel" /> class.
        /// </summary>
        /// <param name="category">The associated <see cref="Category" /></param>
        public ReferenceDataItemRowViewModel(T thing)
        {
            this.Thing = thing;
            this.Name = thing.Name;
            this.ShortName = thing.ShortName;
            var container = (ReferenceDataLibrary)thing.Container;
            this.ContainerName = container.ShortName;
            this.IsDeprecated = thing.IsDeprecated;
        }

        /// <summary>
        /// The represented <see cref="Category" />
        /// </summary>
        public T Thing { get; protected set; }

        /// <summary>
        /// The name of the <see cref="Category" />
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// The short name of the <see cref="Category" />
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// The <see cref="Category" /> container name
        /// </summary>
        public string ContainerName
        {
            get => this.containerName;
            set => this.RaiseAndSetIfChanged(ref this.containerName, value);
        }

        /// <summary>
        /// Value indicating if the <see cref="Category" /> is deprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Backing field for <see cref="IsAllowedToWrite" />
        /// </summary>
        private bool isAllowedToWrite;

        /// <summary>
        /// Value indicating if the <see cref="Category" /> is deprecated
        /// </summary>
        public bool IsAllowedToWrite
        {
            get => this.isAllowedToWrite;
            set => this.RaiseAndSetIfChanged(ref this.isAllowedToWrite, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="categoryRow">The <see cref="CategoryRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ReferenceDataItemRowViewModel<T> categoryRow)
        {
            this.Name = categoryRow.Name;
            this.ShortName = categoryRow.ShortName;
            this.ContainerName = categoryRow.ContainerName;
            this.IsDeprecated = categoryRow.IsDeprecated;
        }
    }
}
