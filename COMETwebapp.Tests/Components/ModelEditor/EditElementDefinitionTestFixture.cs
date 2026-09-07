// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditElementDefinitionTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ModelEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditElementDefinitionTestFixture
    {
        /// <summary>
        /// The bunit <see cref="BunitContext" /> used to render the component under test.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IEditElementDefinitionViewModel" /> bound to the component.
        /// </summary>
        private Mock<IEditElementDefinitionViewModel> viewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" /> registered in the DI container, whose
        /// <see cref="ISessionService.IsReadOnly" /> drives the Save button and read-only notice.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// Initializes the bunit context and a mock view model wrapping a fresh <see cref="ElementDefinition" />.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.viewModel = new Mock<IEditElementDefinitionViewModel>();
            this.viewModel.Setup(x => x.ElementDefinition).Returns(new ElementDefinition { Iid = Guid.NewGuid() });
            this.viewModel.Setup(x => x.AvailableCategories).Returns([]);
            this.viewModel.Setup(x => x.AvailableLanguages).Returns([]);
            this.viewModel.Setup(x => x.DomainOfExpertiseSelectorViewModel).Returns(new Mock<IDomainOfExpertiseSelectorViewModel>().Object);
        }

        /// <summary>
        /// Disposes of the bunit context.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifySaveButtonAndReadOnlyNoticeAreGatedByReadOnlySession()
        {
            var writable = this.context.Render<EditElementDefinition>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var writableButtons = writable.FindComponents<DxButton>().Select(b => b.Instance.Text).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(writableButtons, Does.Contain("Save"), "The Save button must render when the session is writable.");
                Assert.That(writable.FindAll("#form-read-only-notice"), Is.Empty, "No read-only notice must render when the session is writable.");
            });

            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);

            var readOnly = this.context.Render<EditElementDefinition>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var readOnlyButtons = readOnly.FindComponents<DxButton>().Select(b => b.Instance.Text).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(readOnlyButtons, Does.Not.Contain("Save"), "The Save button must be withdrawn when the session is read-only.");
                Assert.That(readOnly.Find("#form-read-only-notice"), Is.Not.Null, "The read-only notice must render when the session is read-only.");
            });
        }
    }
}
