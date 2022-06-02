using CDP4Common.EngineeringModelData;
using NUnit.Framework;
using COMETwebapp.IterationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using CDP4Common.SiteDirectoryData;
using CDP4Common.CommonData;

namespace COMETwebapp.Tests.IterationServiceTest
{
    [TestFixture]
    public class IterationServiceTest
    {
        private Iteration iteration;
        private IterationService iterationService;

        [SetUp]
        public void SetUp()
        {
            var uri = new Uri("http://www.rheagroup.com");
            var cache = new ConcurrentDictionary<CDP4Common.Types.CacheKey, Lazy<Thing>>();

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), cache, uri)
            {
                ShortName = "SYS",
                Name = "System"
            };

            this.iteration = new Iteration(Guid.NewGuid(), cache, uri);

            var option_A = new Option(Guid.NewGuid(), cache, uri)
            {
                ShortName = "OPT_A",
                Name = "Option A"
            };

            var elementDefinition_1 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                ShortName = "Sat",
                Name = "Satellite"
            };

            var elementDefinition_2 = new ElementDefinition(Guid.NewGuid(), cache, uri)
            {
                ShortName = "Bat",
                Name = "Battery"
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
                ShortName = "m"
            };

            var simpleQuantityKind2 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "v"
            };

            var parameter = new Parameter(Guid.NewGuid(), cache, uri)
            {
                Owner = domainOfExpertise,
                ParameterType = simpleQuantityKind
            };

            var parameter2 = new Parameter(Guid.NewGuid(), cache, uri)
            {
                Owner = domainOfExpertise,
                ParameterType = simpleQuantityKind2
            };

            var parameterValueset_1 = new ParameterValueSet()
            {
                ActualOption = option_A,
                Iid = Guid.NewGuid()
            };

            var parameterValueset_2 = new ParameterValueSet()
            {
                ActualOption = option_A,
                Iid = Guid.NewGuid()
            };

            var values_1 = new List<string> { "2" };
            var values_2 = new List<string> { "3" };
            var publishedValues = new List<string> { "123" };

            this.iteration.Option.Add(option_A);
            this.iteration.DefaultOption = option_A;

            parameterValueset_1.Manual = new CDP4Common.Types.ValueArray<string>(values_1);
            parameterValueset_1.Reference = new CDP4Common.Types.ValueArray<string>(values_1);
            parameterValueset_1.Computed = new CDP4Common.Types.ValueArray<string>(values_1);
            parameterValueset_1.Formula = new CDP4Common.Types.ValueArray<string>(values_1);
            parameterValueset_1.Published = new CDP4Common.Types.ValueArray<string>(publishedValues);
            parameterValueset_1.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterValueset_2.Manual = new CDP4Common.Types.ValueArray<string>(values_2);
            parameterValueset_2.Reference = new CDP4Common.Types.ValueArray<string>(values_2);
            parameterValueset_2.Computed = new CDP4Common.Types.ValueArray<string>(values_2);
            parameterValueset_2.Formula = new CDP4Common.Types.ValueArray<string>(values_2);
            parameterValueset_2.Published = new CDP4Common.Types.ValueArray<string>(publishedValues);
            parameterValueset_2.ValueSwitch = ParameterSwitchKind.MANUAL;


            parameter.ValueSet.Add(parameterValueset_1);
            parameter.ValueSet.Add(parameterValueset_2);

            elementDefinition_1.Parameter.Add(parameter);
            elementDefinition_1.ContainedElement.Add(elementUsage_1);
            elementDefinition_1.ContainedElement.Add(elementUsage_2);

            elementDefinition_2.Parameter.Add(parameter);
            elementDefinition_2.Parameter.Add(parameter2);

            this.iteration.Element.Add(elementDefinition_1);
            this.iteration.Element.Add(elementDefinition_2);
            this.iteration.TopElement = elementDefinition_1;

            this.iterationService = new IterationService(this.iteration);
        }

        [Test]
        public void VerifyGetParameterValueSets()
        {
            Assert.IsNotEmpty(iterationService.GetParameterValueSets());
        }

        [Test]
        public void VerifyGetNestedElements()
        {
            Assert.IsNotEmpty(iterationService.GetNestedElements());
        }

        [Test]
        public void VerifyGetNestedParameters()
        {
            Assert.IsNotEmpty(iterationService.GetNestedParameters(this.iteration.Option.First().Iid));
        }

        [Test]
        public void VerifyGetUnusedElementDefinitions()
        {
            Assert.IsNotEmpty(iterationService.GetUnusedElementDefinitions());
        }

        [Test]
        public void VerifyGetUnreferencedElements()
        {
            Assert.IsNotEmpty(iterationService.GetUnreferencedElements());
        }
    }
}
