﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.SystemRepresentation
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ElementDefinitionTestFixture
    {
        private TestContext context;
        private ElementDefinitionDetailsViewModel elementDefinitionDetailsViewModel;
        private Assembler assembler;
        private readonly Uri uri = new ("http://test.com");
        private DomainOfExpertise domain;
        private Iteration iteration;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.elementDefinitionDetailsViewModel = new ElementDefinitionDetailsViewModel();

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Element =
                {
                    new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        Name = "TestElement",
                        Owner = this.domain,
                        ShortName = "TE"
                    }
                }
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyComponent()
        {
            var renderer = this.context.RenderComponent<ElementDefinitionDetails>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.elementDefinitionDetailsViewModel);
            });

            Assert.That(renderer.Instance, Is.Not.Null);
            this.elementDefinitionDetailsViewModel.SelectedSystemNode = this.iteration.Element[0];

            renderer.Render();

            var elementDefinitionDetails = renderer.FindAll("tr");

            Assert.Multiple(() =>
            {
                Assert.That(elementDefinitionDetails[1].InnerHtml, Is.Not.Null);
                Assert.That(elementDefinitionDetails[1].InnerHtml, Does.Contain(this.iteration.Element[0].Name));
            });
        }
    }
}
