// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBodyTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.ParameterEditor
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.Components.ParameterEditor.BatchParameterEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterEditorBodyTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterEditorBody> renderedComponent;
        private ParameterEditorBody editor;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.messageBus = new CDPMessageBus();
            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(new SiteDirectory());
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            sessionService.Setup(x => x.Session).Returns(session.Object);
            var parameterEditorViewModel = new Mock<IParameterEditorBodyViewModel>();

            var elements = new SourceList<ElementBase>();
            elements.Add(new ElementDefinition { Name = "Element1" });
            elements.Add(new ElementDefinition { Name = "Element2" });
            elements.Add(new ElementDefinition { Name = "Element3" });

            parameterEditorViewModel.Setup(x => x.ElementSelector).Returns(new ElementBaseSelectorViewModel());
            parameterEditorViewModel.Setup(x => x.OptionSelector).Returns(new OptionSelectorViewModel());
            parameterEditorViewModel.Setup(x => x.ParameterTypeSelector).Returns(new ParameterTypeSelectorViewModel());
            parameterEditorViewModel.Setup(x => x.ParameterTableViewModel).Returns(new ParameterTableViewModel(sessionService.Object, this.messageBus));

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>()).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(parameterEditorViewModel.Object);

            var parameterTableViewModelMock = new Mock<IParameterTableViewModel>();

            var rowViewModels = new SourceList<ParameterBaseRowViewModel>();

            var parameter = new Parameter
            {
                ParameterType = new ArrayParameterType { Name = "Orientation", ShortName = "orient" },
                Owner = new DomainOfExpertise { Name = "DoE1", ShortName = "doe1" },
                Container = elements.Items.First()
            };

            var actualFiniteStateList = new ActualFiniteStateList
            {
                Iid = Guid.NewGuid()
            };

            var possibleFiniteStateList = new List<PossibleFiniteState> { new() { Iid = Guid.NewGuid(), Name = "State1" }, new() { Iid = Guid.NewGuid(), Name = "State2" } };

            actualFiniteStateList.PossibleFiniteStateList.Add(new PossibleFiniteStateList
            {
                PossibleState = { possibleFiniteStateList[0], possibleFiniteStateList[1] }
            });

            actualFiniteStateList.ActualState.Add(new ActualFiniteState { Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[0] } });
            actualFiniteStateList.ActualState.Add(new ActualFiniteState { Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[1] } });

            var iteration = new Iteration();
            iteration.Option.Add(new Option());
            iteration.ActualFiniteStateList.Add(actualFiniteStateList);

            var valueSet = new Mock<IValueSet>();
            valueSet.Setup(x => x.ValueSwitch).Returns(ParameterSwitchKind.MANUAL);
            valueSet.Setup(x => x.ActualState).Returns(actualFiniteStateList.ActualState[0]);
            valueSet.Setup(x => x.ActualOption).Returns(new Option { Name = "option" });

            rowViewModels.Add(new ParameterBaseRowViewModel(sessionService.Object, false, parameter, valueSet.Object, this.messageBus));

            parameterTableViewModelMock.Setup(x => x.Rows).Returns(rowViewModels);
            this.context.Services.AddSingleton(parameterTableViewModelMock.Object);

            var batchParameterEditorViewModelMock = new Mock<IBatchParameterEditorViewModel>();
            parameterEditorViewModel.Setup(x => x.BatchParameterEditorViewModel).Returns(batchParameterEditorViewModelMock.Object);
            this.context.Services.AddSingleton(batchParameterEditorViewModelMock.Object);

            this.renderedComponent = this.context.RenderComponent<ParameterEditorBody>();
            this.editor = this.renderedComponent.Instance;
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
            Assert.Multiple(() =>
            {
                Assert.That(this.editor, Is.Not.Null);
                Assert.That(this.editor.ViewModel, Is.Not.Null);
                Assert.That(this.renderedComponent, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyComponentUi()
        {
            var elementFilterCombo = this.renderedComponent.FindComponent<ElementBaseSelector>();
            var parameterFilterCombo = this.renderedComponent.FindComponent<ParameterTypeSelector>();
            var optionFilterCombo = this.renderedComponent.FindComponent<OptionSelector>();

            var isOwnedCheckbox = this.renderedComponent.FindComponent<DxCheckBox<bool>>();
            var parameterTable = this.renderedComponent.FindComponent<ParameterTable>();
            var batchParameterEditor = this.renderedComponent.FindComponent<BatchParameterEditor>();

            Assert.Multiple(() =>
            {
                Assert.That(elementFilterCombo, Is.Not.Null);
                Assert.That(parameterFilterCombo, Is.Not.Null);
                Assert.That(optionFilterCombo, Is.Not.Null);
                Assert.That(isOwnedCheckbox, Is.Not.Null);
                Assert.That(parameterTable, Is.Not.Null);
                Assert.That(batchParameterEditor, Is.Not.Null);
            });
        }
    }
}
