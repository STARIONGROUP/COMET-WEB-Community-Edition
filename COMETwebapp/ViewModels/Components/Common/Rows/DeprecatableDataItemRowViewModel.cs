// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeprecatableDataItemRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.Rows
{
    using CDP4Common.CommonData;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for a thing
    /// </summary>
    public abstract class DeprecatableDataItemRowViewModel<T> : ReactiveObject where T : Thing, IShortNamedThing, INamedThing, IDeprecatableThing
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
        /// Initializes a new instance of the <see cref="DeprecatableDataItemRowViewModel{T}" /> class.
        /// </summary>
        /// <param name="thing">The associated thing</param>
        protected DeprecatableDataItemRowViewModel(T thing)
        {
            this.Thing = thing;
            this.Name = thing.Name;
            this.ShortName = thing.ShortName;
            var container = (IShortNamedThing)thing.Container;
            this.ContainerName = container.ShortName;
            this.IsDeprecated = thing.IsDeprecated;
        }

        /// <summary>
        /// The represented thing
        /// </summary>
        public T Thing { get; protected set; }

        /// <summary>
        /// The name of the thing
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// The short name of the thing
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// The thing's container name
        /// </summary>
        public string ContainerName
        {
            get => this.containerName;
            set => this.RaiseAndSetIfChanged(ref this.containerName, value);
        }

        /// <summary>
        /// Value indicating if the thing is deprecated
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
        /// Value indicating if the user is allowed to write the thing
        /// </summary>
        public bool IsAllowedToWrite
        {
            get => this.isAllowedToWrite;
            set => this.RaiseAndSetIfChanged(ref this.isAllowedToWrite, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="thingRow">The <see cref="DeprecatableDataItemRowViewModel{T}" /> to use for updating</param>
        public void UpdateProperties(DeprecatableDataItemRowViewModel<T> thingRow)
        {
            this.Name = thingRow.Name;
            this.ShortName = thingRow.ShortName;
            this.ContainerName = thingRow.ContainerName;
            this.IsDeprecated = thingRow.IsDeprecated;
        }
    }
}
