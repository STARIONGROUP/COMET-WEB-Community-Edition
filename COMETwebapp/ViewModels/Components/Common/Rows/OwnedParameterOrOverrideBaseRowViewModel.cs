﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OwnedParameterOrOverrideBaseRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for owned <see cref="ParameterOrOverrideBase" />
    /// </summary>
    public class OwnedParameterOrOverrideBaseRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="HasMissingValues" />
        /// </summary>
        private bool hasMissingValues;

        /// <summary>
        /// Backing field for <see cref="InterestedDomains" />
        /// </summary>
        private IEnumerable<DomainOfExpertise> interestedDomains;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedParameterOrOverrideBaseRowViewModel" /> class.
        /// </summary>
        public OwnedParameterOrOverrideBaseRowViewModel(ParameterOrOverrideBase parameterBase)
        {
            this.Element = (ElementBase)parameterBase.Container;
            this.Parameter = parameterBase;
            this.InterestedDomains = parameterBase.ParameterSubscription.Select(x => x.Owner);

            this.HasMissingValues = parameterBase.ValueSets.OfType<ParameterValueSetBase>()
                .Any(x => x.Published.All(p => p == "-"));
        }

        /// <summary>
        /// Value indicating if the <see cref="ParameterOrOverrideBase" /> has a <see cref="ParameterValueSetBase" /> with a missing value
        /// </summary>
        public bool HasMissingValues
        {
            get => this.hasMissingValues;
            set => this.RaiseAndSetIfChanged(ref this.hasMissingValues, value);
        }

        /// <summary>
        /// A collection of <see cref="DomainOfExpertise" /> that have <see cref="ParameterSubscription" />
        /// </summary>
        public IEnumerable<DomainOfExpertise> InterestedDomains
        {
            get => this.interestedDomains;
            set => this.RaiseAndSetIfChanged(ref this.interestedDomains, value);
        }

        /// <summary>
        /// A concatenation of all shortname of <see cref="DomainOfExpertise" />
        /// </summary>
        public string InterestedDomainsShortNames => this.InterestedDomains.Select(x => x.ShortName).AsCommaSeparated();

        /// <summary>
        /// The represented <see cref="ParameterOrOverrideBase" />
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; set; }

        /// <summary>
        /// The name of the <see cref="ParameterOrOverrideBase" />
        /// </summary>
        public string ParameterName => this.Parameter.ParameterType.Name;

        /// <summary>
        /// The model code of the <see cref="Parameter"/>
        /// </summary>
        public string ParameterModelCode => this.Parameter.ModelCode();

        /// <summary>
        /// The shortname of the <see cref="DomainOfExpertise" /> owner of the current <see cref="Parameter"/>
        /// </summary>
        public string OwnerDomainShortName => this.Parameter.Owner.ShortName;

        /// <summary>
        /// The published value of the <see cref="Parameter"/>
        /// </summary>
        public string PublishedValue => ((ParameterValueSetBase)this.Parameter.ValueSets.FirstOrDefault())?.Published.ToString();

        /// <summary>
        /// The published value of the <see cref="Parameter"/>
        /// </summary>
        public string ActualValue => this.Parameter.ValueSets.First().ActualValue.ToString();

        /// <summary>
        /// The <see cref="ElementBase" /> container of the <see cref="ParameterOrOverrideBase" />
        /// </summary>
        public ElementBase Element { get; }

        /// <summary>
        /// The name of the <see cref="ElementBase" />
        /// </summary>
        public string ElementName => this.Element.Name;

        /// <summary>
        /// Value indicating if the <see cref="ParameterOrOverrideBase" /> is <see cref="ActualFiniteState" /> dependent
        /// </summary>
        public bool IsStateDependent => this.Parameter.StateDependence != null;

        /// <summary>
        /// Value indicating if the <see cref="ParameterOrOverrideBase" /> is <see cref="Option" /> dependent
        /// </summary>
        public bool IsOptionDependent => this.Parameter.IsOptionDependent;

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="ownedParameter">The <see cref="OwnedParameterOrOverrideBaseRowViewModel" /> to use for updating</param>
        public void UpdateProperties(OwnedParameterOrOverrideBaseRowViewModel ownedParameter)
        {
            this.Parameter = ownedParameter.Parameter;
            this.InterestedDomains = this.Parameter.ParameterSubscription.Select(x => x.Owner);

            this.HasMissingValues = this.Parameter.ValueSets.OfType<ParameterValueSetBase>()
                .Any(x => x.Published.All(p => p == "-"));
        }
    }
}
