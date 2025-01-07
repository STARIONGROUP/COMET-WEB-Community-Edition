// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementBaseTreeRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.MultiModelEditor.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="ElementBase" />
    /// </summary>
    public abstract class ElementBaseTreeRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="CDP4Common.EngineeringModelData.ElementBase" />
        /// </summary>
        private ElementBase elementBase;

        /// <summary>
        /// Backing field for <see cref="ElementName" />
        /// </summary>
        private string elementName;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel" /> class.
        /// <param name="elementBase">the <see cref="CDP4Common.EngineeringModelData.ElementBase" /></param>
        /// </summary>
        protected ElementBaseTreeRowViewModel(ElementBase elementBase)
        {
            ArgumentNullException.ThrowIfNull(elementBase);

            this.ElementBase = elementBase;
            this.ElementName = elementBase.Name;
            this.OwnerShortName = elementBase.Owner.ShortName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementBaseTreeRowViewModel" /> class.
        /// </summary>
        protected ElementBaseTreeRowViewModel()
        {
        }

        /// <summary>
        /// The shortname of the owning <see cref="DomainOfExpertise" />
        /// </summary>
        public string OwnerShortName
        {
            get => this.ownerShortName;
            set => this.RaiseAndSetIfChanged(ref this.ownerShortName, value);
        }

        /// <summary>
        /// The name of the <see cref="CDP4Common.EngineeringModelData.ElementBase" />
        /// </summary>
        public string ElementName
        {
            get => this.elementName;
            set => this.RaiseAndSetIfChanged(ref this.elementName, value);
        }

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.ElementBase" />
        /// </summary>
        public ElementBase ElementBase
        {
            get => this.elementBase;
            set => this.RaiseAndSetIfChanged(ref this.elementBase, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="elementBaseTreeRow">The <see cref="ElementBaseTreeRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ElementBaseTreeRowViewModel elementBaseTreeRow)
        {
            ArgumentNullException.ThrowIfNull(elementBaseTreeRow);

            this.ElementBase = elementBaseTreeRow.elementBase;
            this.ElementName = elementBaseTreeRow.elementName;
            this.OwnerShortName = elementBaseTreeRow.OwnerShortName;
        }
    }
}
