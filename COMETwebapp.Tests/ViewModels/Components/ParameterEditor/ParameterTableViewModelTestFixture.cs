// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMETwebapp.Extensions;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTableViewModelTestFixture
    {
        private ParameterTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private IEnumerable<ElementBase> elementBases;
        private DomainOfExpertise domain;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.viewModel = new ParameterTableViewModel(this.sessionService.Object);
            this.elementBases = new List<ElementBase>();
            
            this.domain = new DomainOfExpertise()
            {
                ShortName = "SYS"
            };

            var parameterType = new SimpleQuantityKind()
            {
                Name = "mass",
                ShortName = "m",
            };

            var scale = new RatioScale()
            {
                Name = "kilogram",
                ShortName = "kg"
            };

            var parameter1 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = scale,
                ValueSet = 
                { 
                    new ParameterValueSet()
                }
            };

            var parameter2 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = scale,
                ValueSet =
                {
                    new ParameterValueSet()
                }
            };

            var parameter3 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = scale,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet()
                }
            };

            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = parameter1,
                ValueSet =
                {
                    new ParameterOverrideValueSet
                    {
                        ParameterValueSet = parameter1.ValueSet[0]
                    }
                }
            };

            var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                Parameter = { parameter3 }
            };

            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                Parameter = { parameter1, parameter2 }
            };

            var usage1 = new ElementUsage()
            {
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDefinition,
                ParameterOverride = { parameterOverride }
            };

            topElement.ContainedElement.Add(usage1);

            this.iteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                Element = { topElement, elementDefinition },
                TopElement = topElement
            };

            this.elementBases = this.iteration.QueryElementsBase();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.elementBases);
            Assert.That(this.viewModel.Rows.Count, Is.EqualTo(4));
        }

        [Test]
        public void VerifyParameterRowProperties()
        {
            this.viewModel.InitializeViewModel(this.elementBases);
            var parameterRow = this.viewModel.Rows.Items.First();
            
            Assert.Multiple(() =>
            {
                Assert.That(parameterRow.OwnerName, Is.EqualTo(this.domain.ShortName));
                Assert.That(parameterRow.ElementBaseName, Is.EqualTo(this.iteration.TopElement.Name));
                Assert.That(parameterRow.ParameterName, Is.EqualTo("mass"));
                Assert.That(parameterRow.Option, Is.Empty);
                Assert.That(parameterRow.State, Is.Empty);
            });
        }

        [Test]
        public async Task VerifyParameterRowBehavior()
        {
            this.viewModel.InitializeViewModel(this.elementBases);
            var parameterRow = this.viewModel.Rows.Items.First();
            var editorViewModel = parameterRow.ParameterTypeEditorSelectorViewModel.CreateParameterEditorViewModel<QuantityKind>();

            Assert.Multiple(() =>
            {
                Assert.That(() => parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.MANUAL, Throws.Nothing);
                Assert.That(parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue != ParameterSwitchKind.MANUAL);
                Assert.That(editorViewModel.CurrentParameterSwitchKind == ParameterSwitchKind.COMPUTED);
            });

            await parameterRow.ParameterSwitchKindSelectorViewModel.OnUpdate.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Never);

            await parameterRow.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync(parameterRow.Parameter.ValueSets.First());
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Never);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.viewModel.InitializeViewModel(this.elementBases);
            parameterRow = this.viewModel.Rows.Items.First();
            editorViewModel = parameterRow.ParameterTypeEditorSelectorViewModel.CreateParameterEditorViewModel<QuantityKind>();

            await parameterRow.ParameterSwitchKindSelectorViewModel.OnUpdate.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Never);

            Assert.Multiple(() =>
            {
                Assert.That(() => parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.MANUAL, Throws.Nothing);
                Assert.That(parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue == ParameterSwitchKind.MANUAL);
                Assert.That(editorViewModel.CurrentParameterSwitchKind == ParameterSwitchKind.MANUAL);
            });

            await parameterRow.ParameterSwitchKindSelectorViewModel.OnUpdate.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Once);

            await parameterRow.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Once);

            await parameterRow.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync(parameterRow.Parameter.ValueSets.First());
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Exactly(2));
        }
    }
}
