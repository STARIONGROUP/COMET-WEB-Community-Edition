// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ThingExtensionsTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Extensions
{
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Extensions;

    using NUnit.Framework;

    [TestFixture]
    public class ThingExtensionsTestFixture
    {
        private Iteration iteration;
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
        }

        [Test]
        public void VerifyGetNestedElements()
        {
            Assert.That(this.iteration.QueryNestedElements(), Is.Not.Empty);
        }

        [Test]
        public void VerifyGetNestedElementsByOption()
        {
            var nestedElements = this.iteration.QueryNestedElements(this.iteration.Option[0]).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(nestedElements, Is.Not.Empty);
                Assert.That(nestedElements, Has.Count.EqualTo(this.iteration.QueryNestedElements().Count()));
            });
        }

        [Test]
        public void VerifyGetParameterTypes()
        {
            var parameterTypes = this.iteration.QueryUsedParameterTypes().ToList();
            Assert.That(parameterTypes, Has.Count.EqualTo(2));

            var parameterTypeNames = new List<string>();
            parameterTypes.ForEach(p => parameterTypeNames.Add(p.Name));

            Assert.Multiple(() =>
            {
                Assert.That(parameterTypeNames, Does.Contain("mass"));
                Assert.That(parameterTypeNames, Does.Contain("volume"));
            });
        }
    }
}
