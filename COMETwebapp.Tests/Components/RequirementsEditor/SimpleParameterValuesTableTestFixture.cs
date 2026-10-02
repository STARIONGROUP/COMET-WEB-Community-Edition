// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SimpleParameterValuesTableTestFixture.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SimpleParameterValuesTableTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<SimpleParameterValuesTable> renderer;
        private Requirement requirement;
        private TextParameterType textParameterType;
        private TextParameterType unusedParameterType;
        private SimpleParameterValue existingValue;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            this.context.Services.AddSingleton<ICDPMessageBus>(new CDPMessageBus());

            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.textParameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "txt", Name = "Text" };
            this.unusedParameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "note", Name = "Note" };

            this.existingValue = new SimpleParameterValue
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.textParameterType,
                Value = new ValueArray<string>(["hello"])
            };

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R01", Name = "Requirement" };
            this.requirement.ParameterValue.Add(this.existingValue);

            this.renderer = this.context.Render<SimpleParameterValuesTable>(parameters => parameters
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.AvailableParameterTypes, new[] { (ParameterType)this.textParameterType, this.unusedParameterType }));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyExistingValueIsRendered()
        {
            Assert.That(this.renderer.Markup, Does.Contain("hello"));
        }

        [Test]
        public async Task VerifyAddValueStagesANewSimpleParameterValue()
        {
            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            await this.renderer.InvokeAsync(() => this.renderer.Instance.OnParameterTypeSelected(this.unusedParameterType));
            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            Assert.Multiple(() =>
            {
                Assert.That(this.requirement.ParameterValue, Has.Count.EqualTo(2));
                Assert.That(this.requirement.ParameterValue.Count(x => x.ParameterType == this.unusedParameterType), Is.EqualTo(1));
                Assert.That(this.renderer.Instance.IsPanelOpen, Is.False, "The panel closes after committing.");
            });
        }

        [Test]
        public async Task VerifyParameterTypeCanOnlyBeUsedOnce()
        {
            Assert.That(this.renderer.Instance.GetSelectableParameterTypes(), Is.EqualTo(new[] { this.unusedParameterType }),
                "A parameter type already used by the requirement is not selectable.");

            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            await this.renderer.InvokeAsync(() => this.renderer.Instance.OnParameterTypeSelected(this.textParameterType));
            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            Assert.That(this.requirement.ParameterValue, Has.Count.EqualTo(1), "A second value for the same parameter type is not added.");

            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            await this.renderer.InvokeAsync(() => this.renderer.Instance.OnParameterTypeSelected(this.unusedParameterType));
            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            Assert.Multiple(() =>
            {
                Assert.That(this.requirement.ParameterValue, Has.Count.EqualTo(2));
                Assert.That(this.renderer.Instance.GetSelectableParameterTypes(), Is.Empty, "Every available parameter type is now used.");
            });
        }

        [Test]
        public async Task VerifyAddPanelRendersInsideAnEditForm()
        {
            // The dialog hosts this table inside an EditForm; the value editors must not crash on the form's EditContext.
            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<EditForm>(0);
                builder.AddAttribute(1, nameof(EditForm.Model), this.requirement);
                builder.AddAttribute(2, nameof(EditForm.ChildContent), (RenderFragment<EditContext>)(_ => childBuilder =>
                {
                    childBuilder.OpenComponent<SimpleParameterValuesTable>(0);
                    childBuilder.AddAttribute(1, nameof(SimpleParameterValuesTable.Requirement), this.requirement);
                    childBuilder.AddAttribute(2, nameof(SimpleParameterValuesTable.AvailableParameterTypes), (IReadOnlyList<ParameterType>)new[] { (ParameterType)this.textParameterType });
                    childBuilder.CloseComponent();
                }));

                builder.CloseComponent();
            };

            var rendered = this.context.Render(fragment);
            var table = rendered.FindComponent<SimpleParameterValuesTable>();

            await rendered.InvokeAsync(table.Instance.OpenAdd);
            await rendered.InvokeAsync(() => table.Instance.OnParameterTypeSelected(this.textParameterType));

            Assert.That(table.Instance.IsPanelOpen, Is.True, "The add panel and its editors must render inside an EditForm without throwing.");
        }

        [Test]
        public async Task VerifyRemoveValue()
        {
            var removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeSimpleParameterValueButton");
            await this.renderer.InvokeAsync(removeButton.Instance.Click.InvokeAsync);

            var removalPopup = this.renderer.Instance.RemovalPopup;

            Assert.Multiple(() =>
            {
                Assert.That(removalPopup.IsVisible, Is.True, "The removal asks for confirmation first.");
                Assert.That(this.requirement.ParameterValue, Has.Count.EqualTo(1), "Nothing is removed before the user confirms.");
            });

            await this.renderer.InvokeAsync(removalPopup.Cancel);

            Assert.Multiple(() =>
            {
                Assert.That(removalPopup.IsVisible, Is.False);
                Assert.That(this.requirement.ParameterValue, Has.Count.EqualTo(1), "Cancelling keeps the value.");
            });

            await this.renderer.InvokeAsync(removeButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(removalPopup.Confirm);

            Assert.Multiple(() =>
            {
                Assert.That(this.requirement.ParameterValue, Is.Empty, "Confirming removes the value.");
                Assert.That(removalPopup.IsVisible, Is.False);
            });
        }

        [Test]
        public async Task VerifyEditValueUpdatesScale()
        {
            await this.renderer.InvokeAsync(() => this.renderer.Instance.OpenEdit(this.existingValue));

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsPanelOpen, Is.True);
                Assert.That(this.renderer.Instance.IsAddMode, Is.False);
            });

            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            // Committing an edit without changing the value keeps the single existing value.
            Assert.That(this.requirement.ParameterValue, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task VerifyQuantityKindSelectionBuildsScaleAndEditor()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "m", Name = "metre" };

            var quantityKind = new SimpleQuantityKind
            {
                Iid = Guid.NewGuid(),
                ShortName = "len",
                Name = "Length",
                PossibleScale = { scale },
                DefaultScale = scale
            };

            this.renderer.Render(parameters => parameters
                .Add(p => p.AvailableParameterTypes, new[] { (ParameterType)this.textParameterType, this.unusedParameterType, quantityKind }));

            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            await this.renderer.InvokeAsync(() => this.renderer.Instance.OnParameterTypeSelected(quantityKind));
            this.renderer.Render();

            Assert.That(this.renderer.Markup, Does.Contain(scale.ShortName), "The scale combo renders the quantity kind's possible scale.");

            var valueTextBox = this.renderer.FindComponents<DxTextBox>().First(x => x.Instance.InputCssClass == "quantity-kind-parameter");
            await this.renderer.InvokeAsync(() => valueTextBox.Instance.TextChanged.InvokeAsync("42"));
            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            var added = this.requirement.ParameterValue.Single(x => x.ParameterType == quantityKind);

            Assert.Multiple(() =>
            {
                Assert.That(added.Value[0], Is.EqualTo("42"), "The value staged through the shared editor is committed.");
                Assert.That(added.Scale, Is.SameAs(scale));
            });
        }

        [Test]
        public void VerifyControlsFollowTheParentWritePermission()
        {
            // These rows are parts of the Requirement and are saved atomically by the hosting form, so the permission is
            // decided once by that form and passed down. Without this a user could stage edits into a dialog whose Save
            // is withdrawn, and only discover it at the end.
            var addButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addSimpleParameterValueButton");
            var removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeSimpleParameterValueButton");

            Assert.Multiple(() =>
            {
                Assert.That(addButton.Instance.Enabled, Is.True, "the default keeps a host that does not set the flag unchanged");
                Assert.That(removeButton.Instance.Enabled, Is.True);
            });

            this.renderer.Render(parameters => parameters
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.AvailableParameterTypes, new[] { (ParameterType)this.textParameterType, this.unusedParameterType })
                .Add(p => p.IsAllowedToWrite, false));

            addButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addSimpleParameterValueButton");
            removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeSimpleParameterValueButton");

            Assert.Multiple(() =>
            {
                Assert.That(addButton.Instance.Enabled, Is.False, "the user may not add values to a thing they cannot write");
                Assert.That(removeButton.Instance.Enabled, Is.False, "the user may not remove values from a thing they cannot write");
            });
        }

        [Test]
        public async Task VerifyClosingThePopupClosesThePanel()
        {
            await this.renderer.InvokeAsync(this.renderer.Instance.OpenAdd);
            this.renderer.Render();
            Assert.That(this.renderer.Instance.IsPanelOpen, Is.True);

            var popup = this.renderer.FindComponents<DxPopup>().First(x => x.Instance.HeaderText.Contains("Simple Parameter Value"));
            await this.renderer.InvokeAsync(() => popup.Instance.VisibleChanged.InvokeAsync(false));

            Assert.That(this.renderer.Instance.IsPanelOpen, Is.False, "Closing the popup (e.g. via its close button) closes the panel.");
        }
    }
}
