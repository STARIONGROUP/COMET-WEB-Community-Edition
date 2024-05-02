// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveDomainsTableTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.Tests.Components.SiteDirectory.EngineeringModels
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ActiveDomainsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ActiveDomainsTable> renderer;
        private Mock<IActiveDomainsTableViewModel> viewModel;
        private EngineeringModelSetup model;
        private DomainOfExpertise domain1;
        private DomainOfExpertise domain2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IActiveDomainsTableViewModel>();
            this.model = new EngineeringModelSetup();

            this.domain1 = new DomainOfExpertise()
            {
                Name = "domain A",
                ShortName = "domainA",
                Container = new EngineeringModelSetup(){ ShortName = "model" },
            };

            this.domain2 = new DomainOfExpertise()
            {
                Name = "domain B",
                ShortName = "domainB",
                Container = new EngineeringModelSetup() { ShortName = "model" },
            };

            var rows = new SourceList<DomainOfExpertiseRowViewModel>();
            rows.Add(new DomainOfExpertiseRowViewModel(this.domain1));
            rows.Add(new DomainOfExpertiseRowViewModel(this.domain2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.Thing).Returns(new DomainOfExpertise());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<ActiveDomainsTable>(p =>
            {
                p.Add(parameter => parameter.EngineeringModelSetup, this.model);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Instance.EngineeringModelSetup, Is.EqualTo(this.model));
                Assert.That(this.renderer.Markup, Does.Contain(this.domain1.Name));
                Assert.That(this.renderer.Markup, Does.Contain(this.domain2.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyEditActiveDomains()
        {
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(false));

            var editActiveDomainsClickableItem = this.renderer.FindComponent<DxToolbarItem>();
            await this.renderer.InvokeAsync(editActiveDomainsClickableItem.Instance.Click.InvokeAsync);

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));

            var saveActiveDomainsButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "saveActiveDomainsButton");
            await this.renderer.InvokeAsync(saveActiveDomainsButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.EditActiveDomains(), Times.Once);

            // Opens the popup again
            await this.renderer.InvokeAsync(editActiveDomainsClickableItem.Instance.Click.InvokeAsync);

            var cancelActiveDomainsButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelActiveDomainsButton");
            await this.renderer.InvokeAsync(cancelActiveDomainsButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.ResetSelectedDomainsOfExpertise(), Times.Once);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(false));
            });
        }
    }
}
