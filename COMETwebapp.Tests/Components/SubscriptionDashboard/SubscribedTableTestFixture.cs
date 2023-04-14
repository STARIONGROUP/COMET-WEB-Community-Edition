// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscribedTableTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Tests.Components.SubscriptionDashboard
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Tests.Helpers;

    using COMETwebapp.Components.SubscriptionDashboard;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Web;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SubscribedTableTestFixture
    {
        private TestContext context;
        private Mock<ISubscribedTableViewModel> viewModel;
        private SourceList<ParameterSubscriptionRowViewModel> rows;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.rows = new SourceList<ParameterSubscriptionRowViewModel>();
            this.viewModel = new Mock<ISubscribedTableViewModel>();
            this.viewModel.Setup(x => x.Rows).Returns(this.rows);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyComponent()
        {
            var renderer = this.context.RenderComponent<SubscribedTable>(parameters =>
                parameters.Add(p => p.ViewModel, this.viewModel.Object));

            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Exception);

            var element = new ElementDefinition();

            var parameter = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = new SimpleQuantityKind(),
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(new[]{"-"}),
                    }
                }
            };

            var parameterSubscription = new ParameterSubscription()
            {
                Owner = new DomainOfExpertise(),
                ValueSet = 
                { 
                    new ParameterSubscriptionValueSet()
                    {
                        SubscribedValueSet = parameter.ValueSet.First()
                    }
                }
            };

            parameter.ParameterSubscription.Add(parameterSubscription);
            element.Parameter.Add(parameter);

            this.rows.Add(new ParameterSubscriptionRowViewModel(parameterSubscription, null, null));
            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Nothing);

            var row = this.rows.Items.First();
            row.Changes[0] = new ValueArray<string>(new[] { "-" });
            row.Changes[1] = new ValueArray<string>(new[] { "-7" });

            renderer.Render();
            var expandButton = renderer.FindComponent<DxButton>();
            await renderer.InvokeAsync(() => expandButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var moreButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Text != null);

            await renderer.InvokeAsync(() => moreButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var parameterEvolution = renderer.FindComponent<SubscribedParameterEvolution>();

            Assert.That(parameterEvolution.Instance.ParameterSubscriptionRow, Is.Not.Null); 
        }

        [Test]
        public async Task VerifyBooleanParameterEvolutionComponent()
        {
            var renderer = this.context.RenderComponent<SubscribedTable>(parameters =>
                parameters.Add(p => p.ViewModel, this.viewModel.Object));

            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Exception);

            var element = new ElementDefinition();

            var parameter = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = new BooleanParameterType(),
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(new[]{"-"}),
                    }
                }
            };

            var parameterSubscription = new ParameterSubscription()
            {
                Owner = new DomainOfExpertise(),
                ValueSet =
                {
                    new ParameterSubscriptionValueSet()
                    {
                        SubscribedValueSet = parameter.ValueSet.First()
                    }
                }
            };

            parameter.ParameterSubscription.Add(parameterSubscription);
            element.Parameter.Add(parameter);

            this.rows.Add(new ParameterSubscriptionRowViewModel(parameterSubscription, null, null));
            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Nothing);

            var row = this.rows.Items.First();
            row.Changes[0] = new ValueArray<string>(new[] { "-" });
            row.Changes[1] = new ValueArray<string>(new[] { "-7" });

            renderer.Render();
            var expandButton = renderer.FindComponent<DxButton>();
            await renderer.InvokeAsync(() => expandButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var moreButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Text != null);

            await renderer.InvokeAsync(() => moreButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var parameterEvolution = renderer.FindComponent<BooleanParameterEvolution>();

            Assert.That(parameterEvolution.Instance.ParameterSubscriptionRow, Is.Not.Null);
        }

        [Test]
        public async Task VerifyEnumerationParameterEvolutionComponent()
        {
            var renderer = this.context.RenderComponent<SubscribedTable>(parameters =>
                parameters.Add(p => p.ViewModel, this.viewModel.Object));

            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Exception);

            var element = new ElementDefinition();

            var enumerationValues = new List<string> { "cube", "sphere", "cylinder" };

            var enumerationData = new OrderedItemList<EnumerationValueDefinition>(null)
            {
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[0],
                    ShortName = "cube"
                },
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[1],
                    ShortName = "sphere"
                },
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[2]
                }
            };

            var enumerationParameterType = new EnumerationParameterType
            {
                Iid = Guid.NewGuid()
            };

            enumerationParameterType.ValueDefinition.AddRange(enumerationData);

            var parameter = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = enumerationParameterType,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(new[]{"-"}),
                    }
                }
            };

            var parameterSubscription = new ParameterSubscription()
            {
                Owner = new DomainOfExpertise(),
                ValueSet =
                {
                    new ParameterSubscriptionValueSet()
                    {
                        SubscribedValueSet = parameter.ValueSet.First()
                    }
                }
            };

            parameter.ParameterSubscription.Add(parameterSubscription);
            element.Parameter.Add(parameter);

            this.rows.Add(new ParameterSubscriptionRowViewModel(parameterSubscription, null, null));
            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Nothing);

            var row = this.rows.Items.First();
            row.Changes[0] = new ValueArray<string>(new[] { "-" });
            row.Changes[1] = new ValueArray<string>(new[] { "cube | sphere" });

            renderer.Render();
            var expandButton = renderer.FindComponent<DxButton>();
            await renderer.InvokeAsync(() => expandButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var moreButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Text != null);

            await renderer.InvokeAsync(() => moreButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var parameterEvolution = renderer.FindComponent<EnumerationParameterEvolution>();

            Assert.That(parameterEvolution.Instance.ParameterSubscriptionRow, Is.Not.Null);
        }
    }
}
