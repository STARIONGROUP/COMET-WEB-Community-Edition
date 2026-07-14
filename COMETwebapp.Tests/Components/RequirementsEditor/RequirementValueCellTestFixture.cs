// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementValueCellTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.Components.RequirementsEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementValueCellTestFixture
    {
        private BunitContext context;
        private CDPMessageBus messageBus;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private EnumerationParameterType enumerationParameterType;
        private Requirement requirement;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.messageBus = new CDPMessageBus();
            this.context.Services.AddSingleton<ICDPMessageBus>(this.messageBus);

            this.enumerationParameterType = new EnumerationParameterType { Iid = Guid.NewGuid(), ShortName = "col", Name = "colour" };
            this.enumerationParameterType.ValueDefinition.Add(new EnumerationValueDefinition { Iid = Guid.NewGuid(), ShortName = "red", Name = "red" });
            this.enumerationParameterType.ValueDefinition.Add(new EnumerationValueDefinition { Iid = Guid.NewGuid(), ShortName = "green", Name = "green" });

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1" };

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyEmptyCellShowsAddAffordance()
        {
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns((SimpleParameterValue)null);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            Assert.Multiple(() =>
            {
                Assert.That(cell.Markup, Does.Contain("req-value-cell-empty"));
                Assert.That(cell.Markup, Does.Contain("req-value-add"));
                Assert.That(cell.Markup, Does.Not.Contain("req-value-editor"));
            });
        }

        [Test]
        public void VerifyClickingValueEntersEditMode()
        {
            var value = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.enumerationParameterType, Value = new ValueArray<string>(["red"]) };
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns(value);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            Assert.That(cell.Markup, Does.Contain("req-value-display"));

            cell.Find(".req-value-display").Click();

            Assert.Multiple(() =>
            {
                Assert.That(cell.Markup, Does.Contain("req-value-editor"));
                Assert.That(cell.Markup, Does.Contain("req-value-save"));
                Assert.That(cell.FindComponents<COMET.Web.Common.Components.ParameterTypeEditors.ParameterTypeEditorSelector>(), Is.Not.Empty);
            });
        }

        [Test]
        public void VerifyAddAffordanceCreatesValue()
        {
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns((SimpleParameterValue)null);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            cell.Find(".req-value-add").Click();

            this.viewModel.Verify(x => x.CreateSimpleParameterValue(this.requirement, this.enumerationParameterType), Times.Once);
        }

        [Test]
        public void VerifySaveCommitsAndLeavesEditMode()
        {
            var value = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.enumerationParameterType, Value = new ValueArray<string>(["red"]) };
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns(value);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            cell.Find(".req-value-display").Click();
            cell.Find(".req-value-save").Click();

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.UpdateSimpleParameterValue(value, It.IsAny<IEnumerable<string>>()), Times.Once);
                Assert.That(cell.Markup, Does.Not.Contain("req-value-editor"), "edit mode is left after saving");
            });
        }

        [Test]
        public void VerifyCancelLeavesEditModeWithoutWriting()
        {
            var value = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.enumerationParameterType, Value = new ValueArray<string>(["red"]) };
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns(value);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            cell.Find(".req-value-display").Click();
            cell.Find(".req-value-cancel").Click();

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.UpdateSimpleParameterValue(It.IsAny<SimpleParameterValue>(), It.IsAny<IEnumerable<string>>()), Times.Never);
                Assert.That(cell.Markup, Does.Not.Contain("req-value-editor"));
            });
        }

        [Test]
        public void VerifyEnterWithoutAStagedChangeDoesNotCommit()
        {
            var value = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.enumerationParameterType, Value = new ValueArray<string>(["red"]) };
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns(value);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            cell.Find(".req-value-display").Click();
            cell.Find(".req-value-editor").KeyDown(new KeyboardEventArgs { Key = "Enter" });

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.UpdateSimpleParameterValue(It.IsAny<SimpleParameterValue>(), It.IsAny<IEnumerable<string>>()), Times.Never, "an Enter that only picks a dropdown item must not be swallowed as a no-op commit");
                Assert.That(cell.Markup, Does.Contain("req-value-editor"), "the editor stays open");
            });
        }

        [Test]
        public void VerifyEscapeCancelsEdit()
        {
            var value = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.enumerationParameterType, Value = new ValueArray<string>(["red"]) };
            this.viewModel.Setup(x => x.GetSimpleParameterValue(this.requirement, this.enumerationParameterType)).Returns(value);

            var cell = this.context.Render<RequirementValueCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.ParameterType, this.enumerationParameterType));

            cell.Find(".req-value-display").Click();
            cell.Find(".req-value-editor").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.UpdateSimpleParameterValue(It.IsAny<SimpleParameterValue>(), It.IsAny<IEnumerable<string>>()), Times.Never);
                Assert.That(cell.Markup, Does.Not.Contain("req-value-editor"));
            });
        }
    }
}
