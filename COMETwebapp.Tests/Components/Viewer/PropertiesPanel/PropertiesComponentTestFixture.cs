﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesComponentTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Tests.Components.Viewer.PropertiesPanel
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PropertiesComponentTestFixture
    {
        private TestContext context;
        private PropertiesComponent properties;
        private IRenderedComponent<PropertiesComponent> renderedComponent;
        private PropertiesComponentViewModel viewModel;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            var babylonService = new Mock<IBabylonInterop>();
            this.context.Services.AddSingleton(babylonService);

            var selectionMediator = new Mock<ISelectionMediator>();
            this.context.Services.AddSingleton(selectionMediator);
            selectionMediator.Setup(x => x.SelectedSceneObjectClone).Returns(new SceneObject(It.IsAny<Primitive>()));

            var sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(sessionService);

            var iterationService = new Mock<ISubscriptionService>();
            this.context.Services.AddSingleton(iterationService);
            this.messageBus = new CDPMessageBus();

            this.viewModel = new PropertiesComponentViewModel(babylonService.Object, sessionService.Object, selectionMediator.Object, this.messageBus)
            {
                IsVisible = true,
                ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet>()
            };

            this.renderedComponent = this.context.RenderComponent<PropertiesComponent>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel);
            });

            this.properties = this.renderedComponent.Instance;
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.properties, Is.Not.Null);
                Assert.That(this.properties.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatComponentCanBeHidden()
        {
            this.properties.ViewModel.IsVisible = true;
            var component = this.renderedComponent.Find("#properties-header");
            Assert.That(component, Is.Not.Null);
            this.properties.ViewModel.IsVisible = false;
            Assert.Throws<ElementNotFoundException>(() => this.renderedComponent.Find("#properties-header"));
        }
        
        [Test]
        public void VerifyElementValueChanges()
        {
            var compoundData = new OrderedItemList<ParameterTypeComponent>(null)
            {
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "firstValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                }
            };

            var parametertype = new CompoundParameterType
            {
                Iid = Guid.NewGuid()
            };

            parametertype.Component.AddRange(compoundData);

            var parameter = new Parameter() { Iid = Guid.NewGuid(), ParameterType = parametertype };

            this.viewModel.SelectedParameter = parameter;

            var compoundValues = new List<string> { "1" };

            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues),
                Container = new Iteration()
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => this.viewModel.ParameterValueSetChanged((parameterValueSet, 0)), Throws.Nothing);
                Assert.That(() => this.viewModel.OnSubmit(), Throws.Nothing);
            });

            var compoundValues1 = new List<string> { "false" };
            
            var parameterValueSet1 = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues1),
            };
     
            Assert.That(() => this.viewModel.ParameterValueSetChanged((parameterValueSet1,0)), Throws.Nothing);                
        }
    }
}
