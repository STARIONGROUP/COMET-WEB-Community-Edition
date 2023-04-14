// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingExtensionTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Extensions;

    using NUnit.Framework;

    [TestFixture]
    public class ThingExtensionTestFixture
    {
        private Iteration iteration;
        private List<ParameterValueSetBase> parameterValueSetBase;
        private List<ParameterSubscription> parameterSubscriptions;
        private List<ElementDefinition> unReferencedElements;
        private List<ElementDefinition> unUsedElements;
        private DomainOfExpertise currentDomainOfExpertise;
        private DomainOfExpertise domainOfExpertise;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void SetUp()
        {
            var uri = new Uri("http://www.rheagroup.com");
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var person = new Person(Guid.NewGuid(), cache, uri);

            var referenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), cache, uri)
            {
                ShortName = "ARDL"
            };

            var participant = new Participant(Guid.NewGuid(), cache, uri)
            {
                Person = person
            };

            var engineeringSetup = new EngineeringModelSetup(Guid.NewGuid(), cache, uri)
            {
                RequiredRdl =
                {
                    referenceDataLibrary
                },
                Participant = { participant }
            };

            this.iteration = new Iteration(Guid.NewGuid(), cache, uri)
            {
                Container = new EngineeringModel(Guid.NewGuid(), cache, uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), cache, uri)
                    {
                        RequiredRdl =
                        {
                            new ModelReferenceDataLibrary(Guid.NewGuid(), cache, uri)
                            {
                                FileType =
                                {
                                    new FileType(Guid.NewGuid(), cache, uri) { Extension = "tar" },
                                    new FileType(Guid.NewGuid(), cache, uri) { Extension = "gz" },
                                    new FileType(Guid.NewGuid(), cache, uri) { Extension = "zip" }
                                }
                            }
                        },
                        Participant = { participant }
                    }
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), cache, uri)
                {
                    Container = engineeringSetup
                },
                DomainFileStore =
                {
                    new DomainFileStore(Guid.NewGuid(), cache, uri) { Owner = this.domainOfExpertise }
                }
            };

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), cache, uri)
            {
                ShortName = "SYS",
                Name = "System"
            };

            this.currentDomainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), cache, uri)
            {
                ShortName = "AOCS",
                Name = "Attitude and orbit control system"
            };

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), cache, uri);
            this.siteDirectory.Person.Add(person);
            this.siteDirectory.Domain.Add(this.domainOfExpertise);
            this.siteDirectory.Domain.Add(this.currentDomainOfExpertise);

            var optionA = new Option(Guid.NewGuid(), cache, uri)
            {
                ShortName = "OPT_A",
                Name = "Option A"
            };

            var elementDefinition1 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ShortName = "Sat",
                Name = "Satellite"
            };

            var elementDefinition2 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ShortName = "Bat",
                Name = "Battery"
            };

            var elementDefinition3 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ShortName = "solar_panel",
                Name = "Solar Panel"
            };

            var elementUsage1 = new ElementUsage(Guid.NewGuid(), cache, uri)
            {
                ElementDefinition = elementDefinition2,
                ShortName = "bat_a",
                Name = "battery a"
            };

            var elementUsage2 = new ElementUsage(Guid.NewGuid(), cache, uri)
            {
                ElementDefinition = elementDefinition2,
                ShortName = "bat_b",
                Name = "battery b"
            };

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "m",
                Name = "mass"
            };

            var simpleQuantityKind2 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "v",
                Name = "volume"
            };

            var parameter = new Parameter(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = simpleQuantityKind
            };

            var parameter2 = new Parameter(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ParameterType = simpleQuantityKind2
            };

            var parameterValueset1 = new ParameterValueSet
            {
                ActualOption = optionA,
                Iid = Guid.NewGuid()
            };

            var parameterValueset2 = new ParameterValueSet
            {
                ActualOption = optionA,
                Iid = Guid.NewGuid()
            };

            var values1 = new List<string> { "2" };
            var values2 = new List<string> { "3" };
            var publishedValues = new List<string> { "123" };

            this.iteration.Option.Add(optionA);
            this.iteration.DefaultOption = optionA;

            parameterValueset1.Manual = new ValueArray<string>(values1);
            parameterValueset1.Reference = new ValueArray<string>(values1);
            parameterValueset1.Computed = new ValueArray<string>(values1);
            parameterValueset1.Formula = new ValueArray<string>(values1);
            parameterValueset1.Published = new ValueArray<string>(publishedValues);
            parameterValueset1.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterValueset2.Manual = new ValueArray<string>(values2);
            parameterValueset2.Reference = new ValueArray<string>(values2);
            parameterValueset2.Computed = new ValueArray<string>(values2);
            parameterValueset2.Formula = new ValueArray<string>(values2);
            parameterValueset2.Published = new ValueArray<string>(publishedValues);
            parameterValueset2.ValueSwitch = ParameterSwitchKind.MANUAL;

            var oldParameterValueset1 = new ParameterValueSet
            {
                ActualOption = optionA,
                Iid = parameterValueset1.Iid,
                Manual = new ValueArray<string>(new List<string> { "3" }),
                Reference = new ValueArray<string>(values1),
                Computed = new ValueArray<string>(values1),
                Formula = new ValueArray<string>(values1),
                Published = new ValueArray<string>(publishedValues),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter.ValueSet.Add(parameterValueset1);
            parameter.ValueSet.Add(parameterValueset2);

            elementDefinition1.Parameter.Add(parameter);
            elementDefinition1.ContainedElement.Add(elementUsage1);
            elementDefinition1.ContainedElement.Add(elementUsage2);

            elementDefinition2.Parameter.Add(parameter2);

            this.iteration.Element.Add(elementDefinition1);
            this.iteration.Element.Add(elementDefinition2);
            this.iteration.Element.Add(elementDefinition3);
            this.iteration.TopElement = elementDefinition1;

            var parameterSubscriptionValueSet = new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var oldParameterSubscriptionValueSet = new ParameterSubscriptionValueSet
            {
                Iid = parameterSubscriptionValueSet.Iid,
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var parameterSubscription = new ParameterSubscription()
            {
                Owner = this.currentDomainOfExpertise,
                ValueSet = { parameterSubscriptionValueSet }
            }; 

            parameter.ParameterSubscription.Add(parameterSubscription);

            this.parameterValueSetBase = new List<ParameterValueSetBase>
            {
                parameterValueset1,
                parameterValueset2
            };

            this.unReferencedElements = new List<ElementDefinition>
            {
                elementDefinition3
            };

            this.unUsedElements = new List<ElementDefinition>
            {
                elementDefinition3
            };

            var nameProperty = typeof(ParameterValueSet).GetProperty(nameof(ParameterValueSet.RevisionNumber))!;
            nameProperty.SetValue(parameterValueset1, 2);
            nameProperty.SetValue(oldParameterValueset1, 1);

            nameProperty = typeof(ParameterSubscriptionValueSet).GetProperty(nameof(ParameterSubscriptionValueSet.RevisionNumber))!;
            nameProperty.SetValue(parameterSubscriptionValueSet, 2);
            nameProperty.SetValue(oldParameterSubscriptionValueSet, 1);

            parameterValueset1.Revisions.Clear();
            parameterValueset1.Revisions.Add(1, oldParameterValueset1);

            parameterSubscriptionValueSet.Revisions.Clear();
            parameterSubscriptionValueSet.Revisions.Add(1, oldParameterSubscriptionValueSet);

            parameterSubscriptionValueSet.SubscribedValueSet = parameterValueset1;

            this.parameterSubscriptions = new List<ParameterSubscription> { parameterSubscription };
        }

        [Test]
        public void VerifyGetNestedParameters()
        {
            Assert.That(this.iteration.QueryNestedParameters(this.iteration.Option[0]), Is.Not.Empty);
        }

        [Test]
        public void VerifyGetParameterSubscriptionsByElement()
        {
            var subscriptions = this.iteration.TopElement.QueryOwnedParameterSubscriptions(this.currentDomainOfExpertise).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(subscriptions, Has.Count.EqualTo(1));
                Assert.That(subscriptions, Does.Contain(this.parameterSubscriptions[0]));
            });
        }

        [Test]
        public void VerifyGetSubscribedParameters()
        {
            var subscriptions = this.iteration.QuerySubscribedParameterByOthers(this.domainOfExpertise).ToList();
            Assert.That(subscriptions, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyGetParameterValueSetBase()
        {
            var valueSets = this.iteration.QueryParameterValueSetBase().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(valueSets, Is.Not.Empty);
                Assert.That(valueSets, Is.EqualTo(this.parameterValueSetBase));
            });
        }

        [Test]
        public void VerifyGetUnreferencedElements()
        {
            var unreferencedElements = this.iteration.QueryUnreferencedElements().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(unreferencedElements, Is.Not.Empty);
                Assert.That(unreferencedElements, Is.EqualTo(this.unReferencedElements));
            });
        }

        [Test]
        public void VerifyGetUnusedElementDefinitions()
        {
            var unusedElements = this.iteration.QueryUnusedElementDefinitions().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(unusedElements, Is.Not.Empty);
                Assert.That(unusedElements, Is.EqualTo(this.unUsedElements));
            });
        }
    }
}
