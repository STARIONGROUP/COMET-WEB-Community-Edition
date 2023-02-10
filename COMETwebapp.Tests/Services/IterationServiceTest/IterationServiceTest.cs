// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationServiceTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace COMETwebapp.Tests.Services.IterationServiceTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Extensions;
    using COMETwebapp.IterationServices;

    using NUnit.Framework;

    [TestFixture]
    public class IterationServiceTest
    {
        private Iteration iteration;
        private readonly IIterationService iterationService = new IterationService();
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

            var option_A = new Option(Guid.NewGuid(), cache, uri)
            {
                ShortName = "OPT_A",
                Name = "Option A"
            };

            var elementDefinition_1 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ShortName = "Sat",
                Name = "Satellite"
            };

            var elementDefinition_2 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ShortName = "Bat",
                Name = "Battery"
            };

            var elementDefinition_3 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                Owner = this.domainOfExpertise,
                ShortName = "solar_panel",
                Name = "Solar Panel"
            };

            var elementUsage_1 = new ElementUsage(Guid.NewGuid(), cache, uri)
            {
                ElementDefinition = elementDefinition_2,
                ShortName = "bat_a",
                Name = "battery a"
            };

            var elementUsage_2 = new ElementUsage(Guid.NewGuid(), cache, uri)
            {
                ElementDefinition = elementDefinition_2,
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

            var parameterValueset_1 = new ParameterValueSet
            {
                ActualOption = option_A,
                Iid = Guid.NewGuid()
            };

            var parameterValueset_2 = new ParameterValueSet
            {
                ActualOption = option_A,
                Iid = Guid.NewGuid()
            };

            var values_1 = new List<string> { "2" };
            var values_2 = new List<string> { "3" };
            var publishedValues = new List<string> { "123" };

            this.iteration.Option.Add(option_A);
            this.iteration.DefaultOption = option_A;

            parameterValueset_1.Manual = new ValueArray<string>(values_1);
            parameterValueset_1.Reference = new ValueArray<string>(values_1);
            parameterValueset_1.Computed = new ValueArray<string>(values_1);
            parameterValueset_1.Formula = new ValueArray<string>(values_1);
            parameterValueset_1.Published = new ValueArray<string>(publishedValues);
            parameterValueset_1.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterValueset_2.Manual = new ValueArray<string>(values_2);
            parameterValueset_2.Reference = new ValueArray<string>(values_2);
            parameterValueset_2.Computed = new ValueArray<string>(values_2);
            parameterValueset_2.Formula = new ValueArray<string>(values_2);
            parameterValueset_2.Published = new ValueArray<string>(publishedValues);
            parameterValueset_2.ValueSwitch = ParameterSwitchKind.MANUAL;

            var oldParameterValueset_1 = new ParameterValueSet
            {
                ActualOption = option_A,
                Iid = parameterValueset_1.Iid
            };

            oldParameterValueset_1.Manual = new ValueArray<string>(new List<string> { "3" });
            oldParameterValueset_1.Reference = new ValueArray<string>(values_1);
            oldParameterValueset_1.Computed = new ValueArray<string>(values_1);
            oldParameterValueset_1.Formula = new ValueArray<string>(values_1);
            oldParameterValueset_1.Published = new ValueArray<string>(publishedValues);
            oldParameterValueset_1.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameter.ValueSet.Add(parameterValueset_1);
            parameter.ValueSet.Add(parameterValueset_2);

            elementDefinition_1.Parameter.Add(parameter);
            elementDefinition_1.ContainedElement.Add(elementUsage_1);
            elementDefinition_1.ContainedElement.Add(elementUsage_2);

            elementDefinition_2.Parameter.Add(parameter2);

            this.iteration.Element.Add(elementDefinition_1);
            this.iteration.Element.Add(elementDefinition_2);
            this.iteration.Element.Add(elementDefinition_3);
            this.iteration.TopElement = elementDefinition_1;

            var parameterSubscriptionValueSet = new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid()
            };

            parameterSubscriptionValueSet.Manual = new ValueArray<string>(new List<string> { "1" });
            parameterSubscriptionValueSet.ValueSwitch = ParameterSwitchKind.MANUAL;

            var oldParameterSubscriptionValueSet = new ParameterSubscriptionValueSet
            {
                Iid = parameterSubscriptionValueSet.Iid
            };

            oldParameterSubscriptionValueSet.Manual = new ValueArray<string>(new List<string> { "1" });
            oldParameterSubscriptionValueSet.ValueSwitch = ParameterSwitchKind.MANUAL;

            var parameterSubscription = new ParameterSubscription();
            parameterSubscription.Owner = this.currentDomainOfExpertise;
            parameterSubscription.ValueSet.Add(parameterSubscriptionValueSet);

            parameter.ParameterSubscription.Add(parameterSubscription);

            this.parameterValueSetBase = new List<ParameterValueSetBase>
            {
                parameterValueset_1,
                parameterValueset_2
            };

            this.unReferencedElements = new List<ElementDefinition>
            {
                elementDefinition_3
            };

            this.unUsedElements = new List<ElementDefinition>
            {
                elementDefinition_3
            };

            PropertyInfo nameProperty = typeof(ParameterValueSet).GetProperty("RevisionNumber");
            nameProperty.SetValue(parameterValueset_1, 2);
            nameProperty.SetValue(oldParameterValueset_1, 1);

            nameProperty = typeof(ParameterSubscriptionValueSet).GetProperty("RevisionNumber");
            nameProperty.SetValue(parameterSubscriptionValueSet, 2);
            nameProperty.SetValue(oldParameterSubscriptionValueSet, 1);

            parameterValueset_1.Revisions.Clear();
            parameterValueset_1.Revisions.Add(1, oldParameterValueset_1);

            parameterSubscriptionValueSet.Revisions.Clear();
            parameterSubscriptionValueSet.Revisions.Add(1, oldParameterSubscriptionValueSet);

            parameterSubscriptionValueSet.SubscribedValueSet = parameterValueset_1;

            this.parameterSubscriptions = new List<ParameterSubscription>();
            this.parameterSubscriptions.Add(parameterSubscription);
        }

        [Test]
        public void VerifyGetNestedElements()
        {
            Assert.That(this.iteration.QueryNestedElements(), Is.Not.Empty);
        }

        [Test]
        public void VerifyGetNestedElementsByOption()
        {
            var option = this.iteration.Option.First();
            Assert.That(this.iteration.QueryNestedElements(option), Is.Not.Empty);
            Assert.That(this.iteration.QueryNestedElements(option).Count, Is.EqualTo(this.iteration.QueryNestedElements().Count()));
        }

        [Test]
        public void VerifyGetNestedParameters()
        {
            Assert.That(this.iteration.QueryNestedParameters(this.iteration.Option.First()), Is.Not.Empty);
        }

        [Test]
        public void VerifyGetNumberUpdates()
        {
            Assert.AreEqual(1, this.iterationService.GetNumberUpdates(this.iteration, this.currentDomainOfExpertise));
            Assert.AreEqual(0, this.iterationService.GetNumberUpdates(this.iteration, this.domainOfExpertise));
        }

        [Test]
        public void VerifyGetParameterSubscriptionsByElement()
        {
            Assert.That(this.iterationService.GetParameterSubscriptionsByElement(this.iteration.TopElement, this.currentDomainOfExpertise), Is.Not.Empty);
            Assert.That(this.iterationService.GetParameterSubscriptionsByElement(this.iteration.TopElement, this.currentDomainOfExpertise).Count, Is.EqualTo(1));
            Assert.That(this.iterationService.GetParameterSubscriptionsByElement(this.iteration.TopElement, this.currentDomainOfExpertise).Contains(this.parameterSubscriptions.First()), Is.True);
        }

        [Test]
        public void VerifyGetParameterTypes()
        {
            var parameterTypes = this.iterationService.GetParameterTypes(this.iteration);
            Assert.That(parameterTypes.Count, Is.EqualTo(2));

            var parameterTypeNames = new List<string>();
            parameterTypes.ForEach(p => parameterTypeNames.Add(p.Name));
            Assert.That(parameterTypeNames.Contains("mass"), Is.True);
            Assert.That(parameterTypeNames.Contains("volume"), Is.True);
        }

        [Test]
        public void VerifyGetParameterValueSetBase()
        {
            Assert.That(this.iteration.QueryParameterValueSetBase(), Is.Not.Empty);
            Assert.That(this.iteration.QueryParameterValueSetBase(), Is.EqualTo(this.parameterValueSetBase));
        }

        [Test]
        public void VerifyGetUnreferencedElements()
        {
            Assert.That(this.iteration.QueryUnreferencedElements(), Is.Not.Empty);
            Assert.That(this.iteration.QueryUnreferencedElements(), Is.EqualTo(this.unReferencedElements));
        }

        [Test]
        public void VerifyGetUnusedElementDefinitions()
        {
            Assert.That(this.iteration.QueryUnusedElementDefinitions(), Is.Not.Empty);
            Assert.That(this.iteration.QueryUnusedElementDefinitions(), Is.EqualTo(this.unUsedElements));
        }
    }
}
