// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSubscriptionTableTestFixture.cs" company="RHEA System S.A.">
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

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DomainOfExpertiseSubscriptionTableTestFixture
    {
        private TestContext context;
        private Mock<IDomainOfExpertiseSubscriptionTableViewModel> viewModel;
        private SourceList<OwnedParameterOrOverrideBaseRowViewModel> rows;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IDomainOfExpertiseSubscriptionTableViewModel>();
            this.rows = new SourceList<OwnedParameterOrOverrideBaseRowViewModel>();
            this.viewModel.Setup(x => x.Rows).Returns(this.rows);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            var renderer = this.context.RenderComponent<DomainOfExpertiseSubscriptionTable>(
                parameters =>
                {
                    parameters.Add(p => p.ViewModel, this.viewModel.Object);
                });

            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Exception);

            var element = new ElementDefinition();

            var parameter = new Parameter()
            {
                ParameterType = new SimpleQuantityKind(),
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(new[] { "-" })
                    }
                }
            };

            var parameterSubscription = new ParameterSubscription();
            parameter.ParameterSubscription.Add(parameterSubscription);
            element.Parameter.Add(parameter);
            this.rows.Add(new OwnedParameterOrOverrideBaseRowViewModel(parameter));
            Assert.That(() => renderer.FindComponent<DxGrid>(), Throws.Nothing);
        }
    }
}
