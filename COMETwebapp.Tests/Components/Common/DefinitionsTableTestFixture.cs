// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefinitionsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Common
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using NUnit.Framework;

    [TestFixture]
    public class DefinitionsTableTestFixture
    {
        private static readonly string[] FrenchLanguageCode = ["fr"];

        private BunitContext context;
        private IRenderedComponent<DefinitionsTable> renderer;
        private TextParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.parameterType = new TextParameterType
            {
                Iid = Guid.NewGuid(),
                ShortName = "txt",
                Name = "text"
            };

            this.parameterType.Definition.Add(new Definition
            {
                Iid = Guid.NewGuid(),
                Content = "Existing definition",
                LanguageCode = "en-GB"
            });

            this.renderer = this.context.Render<DefinitionsTable>(parameters => parameters
                .Add(p => p.Thing, this.parameterType));
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyExistingDefinitionIsRendered()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Thing, Is.SameAs(this.parameterType));
                Assert.That(this.renderer.Markup, Does.Contain("Existing definition"));
                Assert.That(this.renderer.Markup, Does.Contain("en-GB"));
            });
        }

        [Test]
        public async Task VerifyAddDefinition()
        {
            var addButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addDefinitionButton");
            await this.renderer.InvokeAsync(addButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.True);
                Assert.That(this.renderer.Instance.Item, Is.Not.Null);
                Assert.That(this.renderer.Instance.Item.LanguageCode, Is.EqualTo("en-GB"));
                Assert.That(this.parameterType.Definition, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyRemoveDefinition()
        {
            var removeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeDefinitionButton");
            await this.renderer.InvokeAsync(removeButton.Instance.Click.InvokeAsync);

            var removalPopup = this.renderer.Instance.RemovalPopup;

            Assert.Multiple(() =>
            {
                Assert.That(removalPopup.IsVisible, Is.True, "The removal asks for confirmation first.");
                Assert.That(this.parameterType.Definition, Has.Count.EqualTo(1), "Nothing is removed before the user confirms.");
            });

            await this.renderer.InvokeAsync(removalPopup.Cancel);

            Assert.Multiple(() =>
            {
                Assert.That(removalPopup.IsVisible, Is.False);
                Assert.That(this.parameterType.Definition, Has.Count.EqualTo(1), "Cancelling keeps the definition.");
            });

            await this.renderer.InvokeAsync(removeButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(removalPopup.Confirm);

            Assert.Multiple(() =>
            {
                Assert.That(this.parameterType.Definition, Is.Empty, "Confirming removes the definition.");
                Assert.That(removalPopup.IsVisible, Is.False);
            });
        }

        [Test]
        public async Task VerifyGetSelectableLanguagesExcludesUsedLanguages()
        {
            var english = new NaturalLanguage { LanguageCode = "en-GB", Name = "English" };
            var french = new NaturalLanguage { LanguageCode = "fr", Name = "French" };

            var languageRenderer = this.context.Render<DefinitionsTable>(parameters => parameters
                .Add(p => p.Thing, this.parameterType)
                .Add(p => p.AvailableLanguages, new[] { english, french }));

            var addButton = languageRenderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addDefinitionButton");
            await languageRenderer.InvokeAsync(addButton.Instance.Click.InvokeAsync);

            // The parent already holds an en-GB definition, so only the unused French language may be selected for the new one.
            Assert.That(languageRenderer.Instance.GetSelectableLanguages().Select(x => x.LanguageCode), Is.EqualTo(FrenchLanguageCode));
        }
    }
}
