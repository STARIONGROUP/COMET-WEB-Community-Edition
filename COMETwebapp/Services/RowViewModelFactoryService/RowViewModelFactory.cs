// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RowViewModelFactoryService.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RowViewModelFactoryService
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// The factory used to create new rows that inherit from <see cref="BaseDataItemRowViewModel{T}"/>
    /// </summary>
    public static class RowViewModelFactory
    {
        /// <summary>
        /// Creates a row of the selected type
        /// </summary>
        /// <param name="thing">The thing to create a row</param>
        /// <returns>The created row</returns>
        public static BaseDataItemRowViewModel<T> CreateRow<T>(T thing) where T : Thing
        {
            object row = thing switch
            {
                // Reference data rows
                Category castedThing => new CategoryRowViewModel(castedThing),
                DependentParameterTypeAssignment castedThing => new DependentParameterTypeRowViewModel(castedThing),
                EnumerationValueDefinition castedThing => new EnumerationValueDefinitionRowViewModel(castedThing),
                IndependentParameterTypeAssignment castedThing => new IndependentParameterTypeRowViewModel(castedThing),
                MappingToReferenceScale castedThing => new MappingToReferenceScaleRowViewModel(castedThing),
                MeasurementScale castedThing => new MeasurementScaleRowViewModel(castedThing),
                MeasurementUnit castedThing => new MeasurementUnitRowViewModel(castedThing),
                ParameterTypeComponent castedThing => new ParameterTypeComponentRowViewModel(castedThing),
                ParameterType castedThing => new ParameterTypeRowViewModel(castedThing),
                QuantityKindFactor castedThing => new QuantityKindFactorRowViewModel(castedThing),
                ScaleValueDefinition castedThing => new ScaleValueDefinitionRowViewModel(castedThing),
                UnitFactor castedThing => new UnitFactorRowViewModel(castedThing),

                // Site directory rows
                DomainOfExpertise castedThing => new DomainOfExpertiseRowViewModel(castedThing),
                EngineeringModelSetup castedThing => new EngineeringModelRowViewModel(castedThing),
                Iteration castedThing => new IterationRowViewModel(castedThing),
                OrganizationalParticipant castedThing => new OrganizationalParticipantRowViewModel(castedThing),
                Organization castedThing => new OrganizationRowViewModel(castedThing),
                ParticipantRole castedThing => new ParticipantRoleRowViewModel(castedThing),
                Participant castedThing => new ParticipantRowViewModel(castedThing),
                PersonRole castedThing => new PersonRoleRowViewModel(castedThing),
                Person castedThing => new PersonRowViewModel(castedThing),

                // Engineering model rows
                CommonFileStore castedThing => new CommonFileStoreRowViewModel(castedThing),
                FileRevision castedThing => new FileRevisionRowViewModel(castedThing),
                FileType castedThing => new FileTypeRowViewModel(castedThing),
                Option castedThing => new OptionRowViewModel(castedThing),
                Publication castedThing => new PublicationRowViewModel(castedThing),
                _ => null
            };

            return (BaseDataItemRowViewModel<T>)row;
        }
    }
}
