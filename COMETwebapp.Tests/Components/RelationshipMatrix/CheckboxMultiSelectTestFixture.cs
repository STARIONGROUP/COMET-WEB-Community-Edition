// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CheckboxMultiSelectTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RelationshipMatrix
{
    using System.Reflection;

    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RelationshipMatrix;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class CheckboxMultiSelectTestFixture
    {
        private static readonly string[] Data = ["a", "b", "c"];

        private BunitContext context;
        private List<string> capturedValues;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.capturedValues = null;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<CheckboxMultiSelect<string>> RenderComponent(IEnumerable<string> values)
        {
            return this.context.Render<CheckboxMultiSelect<string>>(parameters => parameters
                .Add(p => p.Data, Data)
                .Add(p => p.Values, values)
                .Add(p => p.TextSelector, (Func<string, string>)(x => x))
                .Add(p => p.NullText, "none")
                .Add(p => p.ValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => this.capturedValues = v?.ToList())));
        }

        [Test]
        public void VerifyNullTextWhenEmpty()
        {
            var renderedComponent = this.RenderComponent([]);

            var trigger = renderedComponent.Find(".checkbox-multiselect-trigger");

            Assert.That(trigger.TextContent.Trim(), Is.EqualTo("none"));
        }

        [Test]
        public void VerifySummaryText()
        {
            var renderedComponent = this.RenderComponent(["a", "b"]);

            var trigger = renderedComponent.Find(".checkbox-multiselect-trigger");

            Assert.That(trigger.TextContent, Does.Contain("a; b"));
        }

        [Test]
        public async Task VerifySelectionHandlersForwardToValuesChanged()
        {
            var renderedComponent = this.RenderComponent([]);

            // The "(Select All)" checkbox and list box live inside the un-rendered DxDropDown body (see the note below),
            // so the forwarding handlers are invoked directly to verify they push the right selection to ValuesChanged.
            await renderedComponent.InvokeAsync(() => InvokePrivate(renderedComponent.Instance, "OnSelectAllChanged", true));
            Assert.That(this.capturedValues, Is.EqualTo(Data));

            await renderedComponent.InvokeAsync(() => InvokePrivate(renderedComponent.Instance, "OnSelectAllChanged", false));
            Assert.That(this.capturedValues, Is.Empty);

            await renderedComponent.InvokeAsync(() => InvokePrivate(renderedComponent.Instance, "OnValuesChanged", new[] { "a" }));

            Assert.Multiple(() =>
            {
                Assert.That(this.capturedValues, Is.EqualTo(new[] { "a" }));
                Assert.That(GetPrivate(renderedComponent.Instance, "AreAllSelected"), Is.False);
            });
        }

        private static Task InvokePrivate(object target, string name, object argument)
        {
            return (Task)target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, [argument]);
        }

        private static object GetPrivate(object target, string name)
        {
            return target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }

        // ponytail: no VerifySelectAllSelectsEverything test. The "(Select All)" DxCheckBox and DxListBox only
        // enter the render tree once the DxDropDown is opened, and opening it in bunit throws a
        // NullReferenceException deep inside DevExpress.Blazor.Internal.ListBoxDataControllerDataClient.GetComplexColumns(),
        // even with ConfigureDevExpressBlazor() applied. That is a bunit/DevExpress 23.2 DxListBox limitation,
        // not something this component's tests can work around; NullText/SummaryText above already exercise
        // CheckboxMultiSelect's own (non-DevExpress-internal) logic.
    }
}
