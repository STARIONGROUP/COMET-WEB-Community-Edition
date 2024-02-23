// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation.Rows
{
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="ElementDefinition" />
    /// </summary>
    public class ElementDefinitionRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ElementDefinitionName" />
        /// </summary>
        private string elementDefinitionName;

        /// <summary>
        /// Backing field for <see cref="ElementUsageName" />
        /// </summary>
        private string elementUsageName;

        /// <summary>
        /// Backing field for <see cref="ElementBase" />
        /// </summary>
        private ElementBase elementBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel" /> class.
        /// <param name="elementBase">the <see cref="ElementBase" /></param>
        /// </summary>
        public ElementDefinitionRowViewModel(ElementBase elementBase)
        {
            this.elementBase = elementBase;

            if (this.elementBase is ElementDefinition elementDefinition)
            {
                this.ElementDefinitionName = elementDefinition.Name;
            }
            else if (this.elementBase is ElementUsage elementUsage)
            {
                var elementDefinitionContainer = elementUsage.GetContainerOfType<ElementDefinition>();
                this.ElementDefinitionName = elementDefinitionContainer.Name;
                this.ElementUsageName = elementUsage.Name;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel" /> class.
        /// </summary>
        public ElementDefinitionRowViewModel()
        {
        }

        /// <summary>
        /// The name of the <see cref="ElementDefinition" />
        /// </summary>
        public string ElementDefinitionName
        {
            get => this.elementDefinitionName;
            set => this.RaiseAndSetIfChanged(ref this.elementDefinitionName, value);
        }

        /// <summary>
        /// The name of the <see cref="ElementUsage" />
        /// </summary>
        public string ElementUsageName
        {
            get => this.elementUsageName;
            set => this.RaiseAndSetIfChanged(ref this.elementUsageName, value);
        }

        /// <summary>
        /// The <see cref="ElementBase"/>
        /// </summary>
        public ElementBase ElementBase
        {
            get => this.elementBase;
            set => this.RaiseAndSetIfChanged(ref this.elementBase, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="elementDefinitionRow">The <see cref="ElementDefinitionRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ElementDefinitionRowViewModel elementDefinitionRow)
        {
            this.ElementBase = elementDefinitionRow.elementBase;
            this.ElementDefinitionName = elementDefinitionRow.elementDefinitionName;
            this.ElementUsageName = elementDefinitionRow.elementUsageName;
        }
    }
}
