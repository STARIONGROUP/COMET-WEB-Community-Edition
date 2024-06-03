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
    /// The factory used to create new rows that inherit from <see cref="BaseDataItemRowViewModel{T}" />
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
                Category castThing => new CategoryRowViewModel(castThing),
                DependentParameterTypeAssignment castThing => new DependentParameterTypeRowViewModel(castThing),
                EnumerationValueDefinition castThing => new EnumerationValueDefinitionRowViewModel(castThing),
                IndependentParameterTypeAssignment castThing => new IndependentParameterTypeRowViewModel(castThing),
                MappingToReferenceScale castThing => new MappingToReferenceScaleRowViewModel(castThing),
                MeasurementScale castThing => new MeasurementScaleRowViewModel(castThing),
                MeasurementUnit castThing => new MeasurementUnitRowViewModel(castThing),
                ParameterTypeComponent castThing => new ParameterTypeComponentRowViewModel(castThing),
                ParameterType castThing => new ParameterTypeRowViewModel(castThing),
                QuantityKindFactor castThing => new QuantityKindFactorRowViewModel(castThing),
                ScaleValueDefinition castThing => new ScaleValueDefinitionRowViewModel(castThing),
                UnitFactor castThing => new UnitFactorRowViewModel(castThing),

                // Site directory rows
                DomainOfExpertise castThing => new DomainOfExpertiseRowViewModel(castThing),
                EngineeringModelSetup castThing => new EngineeringModelRowViewModel(castThing),
                Iteration castThing => new IterationRowViewModel(castThing),
                OrganizationalParticipant castThing => new OrganizationalParticipantRowViewModel(castThing),
                Organization castThing => new OrganizationRowViewModel(castThing),
                ParticipantRole castThing => new ParticipantRoleRowViewModel(castThing),
                Participant castThing => new ParticipantRowViewModel(castThing),
                PersonRole castThing => new PersonRoleRowViewModel(castThing),
                Person castThing => new PersonRowViewModel(castThing),

                // Engineering model rows
                CommonFileStore castThing => new CommonFileStoreRowViewModel(castThing),
                DomainFileStore castThing => new DomainFileStoreRowViewModel(castThing),
                FileRevision castThing => new FileRevisionRowViewModel(castThing),
                FileType castThing => new FileTypeRowViewModel(castThing),
                Option castThing => new OptionRowViewModel(castThing),
                Publication castThing => new PublicationRowViewModel(castThing),
                _ => throw new NotSupportedException()
            };

            return (BaseDataItemRowViewModel<T>)row;
        }
    }
}
