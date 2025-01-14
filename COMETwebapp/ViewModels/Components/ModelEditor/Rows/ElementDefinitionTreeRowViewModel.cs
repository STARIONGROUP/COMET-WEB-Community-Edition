// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.Rows
{
    using System.Collections.ObjectModel;

    using CDP4Common.EngineeringModelData;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="ElementDefinition" />
    /// </summary>
    public class ElementDefinitionTreeRowViewModel : ElementBaseTreeRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsTopElement" />
        /// </summary>
        private bool isTopElement;

        /// <summary>
        /// Gets or the collection of <see cref="ElementUsageTreeRowViewModel"/>
        /// </summary>
        public ObservableCollection<ElementUsageTreeRowViewModel> Rows { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel" /> class.
        /// <param name="elementBase">the <see cref="ElementBase" /></param>
        /// </summary>
        public ElementDefinitionTreeRowViewModel(ElementDefinition elementBase) : base(elementBase)
        {
            ArgumentNullException.ThrowIfNull(elementBase);

            this.IsTopElement = elementBase == elementBase.GetContainerOfType<Iteration>().TopElement;

            var elementUsages = elementBase.ContainedElement;

            if (elementUsages?.Any() ?? false)
            {
                this.Rows.AddRange(elementUsages.Select(x => new ElementUsageTreeRowViewModel(x)).OrderBy(x => x.ElementName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionTreeRowViewModel" /> class.
        /// </summary>
        public ElementDefinitionTreeRowViewModel()
        {
        }

        /// <summary>
        /// The value to check if the element base is the top element
        /// </summary>
        public bool IsTopElement
        {
            get => this.isTopElement;
            set => this.RaiseAndSetIfChanged(ref this.isTopElement, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="elementDefinitionTreeRow">The <see cref="ElementDefinitionTreeRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ElementDefinitionTreeRowViewModel elementDefinitionTreeRow)
        {
            ArgumentNullException.ThrowIfNull(elementDefinitionTreeRow);

            base.UpdateProperties(elementDefinitionTreeRow);
            this.IsTopElement = elementDefinitionTreeRow.isTopElement;

            this.Rows.Clear();

            var elementUsages = (elementDefinitionTreeRow.ElementBase as ElementDefinition)?.ContainedElement;

            if (elementUsages != null)
            {
                this.Rows.AddRange(elementUsages.Select(x => new ElementUsageTreeRowViewModel(x)).OrderBy(x => x.ElementName));
            }
        }
    }
}
