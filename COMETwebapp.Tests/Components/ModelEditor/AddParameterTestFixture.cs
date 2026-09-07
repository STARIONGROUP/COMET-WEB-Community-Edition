// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AddParameterTestFixture.cs" company="Starion Group S.A.">
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
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AddParameterTestFixture
    {
        /// <summary>
        /// The bunit <see cref="BunitContext" /> used to render the component under test.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IAddParameterViewModel" /> bound to the component.
        /// </summary>
        private Mock<IAddParameterViewModel> viewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" /> registered in the DI container, whose
        /// <see cref="ISessionService.IsReadOnly" /> drives the Create button.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// Initializes the bunit context and a mock view model wrapping a fresh <see cref="Parameter" />.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.viewModel = new Mock<IAddParameterViewModel>();
            this.viewModel.Setup(x => x.Parameter).Returns(new Parameter { Iid = Guid.NewGuid(), ParameterType = new BooleanParameterType() });
            this.viewModel.Setup(x => x.ParameterTypeSelectorViewModel).Returns(new Mock<IParameterTypeSelectorViewModel>().Object);
            this.viewModel.Setup(x => x.DomainOfExpertiseSelectorViewModel).Returns(new Mock<IDomainOfExpertiseSelectorViewModel>().Object);
            this.viewModel.Setup(x => x.PossibleFiniteStates).Returns([]);
            this.viewModel.Setup(x => x.ParameterGroups).Returns([]);
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
        public void VerifyCreateButtonIsGatedByReadOnlySession()
        {
            var writable = this.context.Render<AddParameter>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));

            Assert.That(writable.FindComponents<DxButton>().Select(b => b.Instance.Text), Does.Contain("Create"),
                "The Create button must render when the session is writable.");

            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);

            var readOnly = this.context.Render<AddParameter>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));

            Assert.That(readOnly.FindComponents<DxButton>().Select(b => b.Instance.Text), Does.Not.Contain("Create"),
                "The Create button must be withdrawn when the session is read-only.");
        }
    }
}
