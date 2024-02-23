// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSubscriptionTableViewModeltTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SubscriptionDashboard
{
    using System;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using NUnit.Framework;

    [TestFixture]
    public class DomainOfExpertiseSubscriptionTableViewModeltTestFixture
    {
        private DomainOfExpertiseSubscriptionTableViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.viewModel = new DomainOfExpertiseSubscriptionTableViewModel();
        }

        [Test]
        public void VerifyUpdatePropertiesAndFilter()
        {
            var domain = new DomainOfExpertise() { Iid = Guid.NewGuid(), Name = "System", ShortName = "SYS" };
            var otherDomain = new DomainOfExpertise() { Iid = Guid.NewGuid(), Name = "Thermal", ShortName = "THE" };

            var parameterType = new TextParameterType()
            {
                Iid = Guid.NewGuid(),
                Name = "answer"
            };

            var parameter = new Parameter()
            {
                Iid = Guid.NewGuid(),
                Owner = domain,
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Iid = Guid.NewGuid(),
                        Published = new ValueArray<string>(new[] { "-" })
                    }
                },
                ParameterType = parameterType
            };

            var subscription = new ParameterSubscription()
            {
                Iid = Guid.NewGuid(),
                Owner = otherDomain
            };

            parameter.ParameterSubscription.Add(subscription);

            var element = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Element",
                Parameter = { parameter }
            };

            this.viewModel.UpdateProperties(element.Parameter);
            var row = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));
                Assert.That(row.ElementName, Is.EqualTo(element.Name));
                Assert.That(row.ParameterName, Is.EqualTo(parameterType.Name));
                Assert.That(row.IsOptionDependent, Is.False);
                Assert.That(row.IsStateDependent, Is.False);
                Assert.That(row.HasMissingValues, Is.True);
                Assert.That(row.InterestedDomainsShortNames, Is.EquivalentTo(otherDomain.ShortName));
            });

            this.viewModel.ApplyFilters(null, new BooleanParameterType());
            Assert.That(this.viewModel.Rows, Is.Empty);

            this.viewModel.ApplyFilters(new Option(), null);
            Assert.That(this.viewModel.Rows, Is.Not.Empty);
        }
    }
}
