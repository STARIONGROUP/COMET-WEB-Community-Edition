// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscribedTableViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.SubscriptionDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Extensions;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SubscribedTableViewModelTestFixture
    {
        private SubscribedTableViewModel viewModel;
        private Mock<ISubscriptionService> subscriptionService;

        [SetUp]
        public void Setup()
        {
            this.subscriptionService = new Mock<ISubscriptionService>();
            this.viewModel = new SubscribedTableViewModel(this.subscriptionService.Object);
        }

        [Test]
        public void VerifyInitialProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Is.Empty);
                Assert.That(this.viewModel.DidSubscriptionsChanged, Is.False);
                Assert.That(this.viewModel.ShowOnlyChangedSubscription, Is.False);
            });
        }

        [Test]
        public void VerifyUpdatePropertiesAndFilter()
        {
            var iteration = new Iteration() { Iid = Guid.NewGuid()};

            var options = new List<Option>() { new() { Iid = Guid.NewGuid(), Name = "Option1"} };

            var possibleFiniteStateList = new List<PossibleFiniteState> { new() { Iid = Guid.NewGuid(), Name = "State1" }, new() { Iid = Guid.NewGuid(), Name = "State2" } };

            var actualFiniteStateList = new ActualFiniteStateList()
            {
                Iid = Guid.NewGuid()
            };

            actualFiniteStateList.PossibleFiniteStateList.Add(new PossibleFiniteStateList()
            {
                PossibleState = { possibleFiniteStateList[0], possibleFiniteStateList[1] }
            });

            actualFiniteStateList.ActualState.Add(new ActualFiniteState() { Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[0] } });
            actualFiniteStateList.ActualState.Add(new ActualFiniteState() { Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[1] } });
            iteration.ActualFiniteStateList.Add(actualFiniteStateList);

            var domain = new DomainOfExpertise() { Iid = Guid.NewGuid(), Name = "System", ShortName = "SYS" };
            var otherDomain = new DomainOfExpertise() { Iid = Guid.NewGuid(), Name = "Thermal", ShortName = "THE" };

            var element = new ElementDefinition() { Iid = Guid.NewGuid(), Name = "Box"};

            var massParameterType = new SimpleQuantityKind() { Iid = Guid.NewGuid(), Name = "mass" };
            var heightParameterType = new SimpleQuantityKind() { Iid = Guid.NewGuid(), Name = "height" };

            var massParameter = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = massParameterType,
                Scale = new RatioScale()
                {
                    Iid = Guid.NewGuid(), 
                    ShortName = "kg"
                }, 
                IsOptionDependent = true,
                StateDependence = actualFiniteStateList,
                Owner = otherDomain,
                ValueSet = 
                { 
                    new ParameterValueSet()
                    {
                        ActualOption = options[0],
                        ActualState = actualFiniteStateList.ActualState[0],
                        Manual = new ValueArray<string>(new []{"45"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    },
                    new ParameterValueSet()
                    {
                        ActualOption = options[0],
                        ActualState = actualFiniteStateList.ActualState[1],
                        Manual = new ValueArray<string>(new []{"45"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var heightParameter = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = heightParameterType,
                Scale = new RatioScale()
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "km"
                },
                Owner = otherDomain,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Manual = new ValueArray<string>(new []{"-"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var massSubscription = new ParameterSubscription()
            {
                Iid = Guid.NewGuid(),
                Owner = domain,
                ValueSet =
                {
                    new ParameterSubscriptionValueSet()
                    {
                        Iid = Guid.NewGuid(),
                        ValueSwitch = ParameterSwitchKind.COMPUTED,
                        SubscribedValueSet = massParameter.ValueSet[0]
                    },
                    new ParameterSubscriptionValueSet()
                    {
                        Iid = Guid.NewGuid(),
                        ValueSwitch = ParameterSwitchKind.COMPUTED,
                        SubscribedValueSet = massParameter.ValueSet[1]
                    }
                }
            };

            massParameter.ParameterSubscription.Add(massSubscription);

            var heightSubscription = new ParameterSubscription()
            {
                Owner = domain,
                ValueSet =
                {
                    new ParameterSubscriptionValueSet()
                    {
                        Iid = Guid.NewGuid(),
                        ValueSwitch = ParameterSwitchKind.COMPUTED,
                        SubscribedValueSet = heightParameter.ValueSet[0]
                    }
                }
            };

            heightParameter.ParameterSubscription.Add(heightSubscription);

            element.Parameter.Add(massParameter);
            element.Parameter.Add(heightParameter);

            var dictionary = new Dictionary<Guid, List<Guid>>();

            this.subscriptionService.Setup(x => x.SubscriptionsWithUpdate).Returns(dictionary.AsReadOnly);

            this.viewModel.UpdateProperties(element.QueryOwnedParameterSubscriptions(domain), options, iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DidSubscriptionsChanged, Is.False);
                Assert.That(this.viewModel.Rows, Has.Count.EqualTo(3));
            });

            dictionary[iteration.Iid] = new List<Guid> { massSubscription.ValueSet[0].Iid };

            this.subscriptionService.Setup(x => x.SubscriptionsWithUpdate).Returns(dictionary.AsReadOnly);
            this.viewModel.UpdateProperties(element.QueryOwnedParameterSubscriptions(domain), options, iteration);

            Assert.That(this.viewModel.DidSubscriptionsChanged, Is.True);

            this.viewModel.ApplyFilters(options[0], heightParameterType);
            Assert.That(this.viewModel.Rows, Is.Empty);

            this.viewModel.ApplyFilters(null, null);
            this.viewModel.ShowOnlyChangedSubscription = true;
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.viewModel.ValidateAllChanges();
            Assert.That(this.viewModel.ShowOnlyChangedSubscription, Is.False);

            var row = this.viewModel.Rows.Items.First();
            
            Assert.Multiple(() =>
            {
                Assert.That(row.ElementName, Is.EqualTo(element.Name));
                Assert.That(row.ParameterName, Is.EqualTo(massParameterType.Name));
                Assert.That(row.OptionName, Is.EqualTo(options[0].Name));
                Assert.That(row.StateName, Is.EqualTo(actualFiniteStateList.ActualState[0].Name));
                Assert.That(row.Changes, Is.Not.Null);
            });
        }
    }
}
