// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DynamicApplicationBaseTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
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

namespace COMET.Web.Common.Tests.Components.Applications
{
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Components.BookEditor;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DynamicApplicationBaseTestFixture
    {
        private Mock<ICustomApplicationBaseViewModel> viewModel;
        private TestContext context;
        private Type componentType;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new Mock<ICustomApplicationBaseViewModel>();
            this.context = new TestContext();
            this.componentType = typeof(CustomApplicationBase);
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(new Mock<IConfigurationService>().Object);
        }

        [Test]
        public void VerifyDynamicApplicationBase()
        {
            var invalidType = typeof(EditorPopup);

            Assert.That(() => this.context.RenderComponent<DynamicApplicationBase>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.ApplicationBaseType, invalidType);
            }), Throws.InvalidOperationException.With.Message.Contain("is not an ApplicationBase"));

            var invalidViewModel = new Mock<IOtherCustomApplicationBaseViewModel>();

            Assert.That(() => this.context.RenderComponent<DynamicApplicationBase>(parameters =>
            {
                parameters.Add(p => p.ViewModel, invalidViewModel.Object);
                parameters.Add(p => p.ApplicationBaseType, this.componentType);
            }), Throws.InvalidOperationException.With.Message.Contain("does not matches the required Type"));

            Assert.That(() => this.context.RenderComponent<DynamicApplicationBase>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.ApplicationBaseType, this.componentType);
            }), Throws.Nothing);
        }
    }

    public class CustomApplicationBase : ApplicationBase<ICustomApplicationBaseViewModel>
    {
        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }
    }

    public interface ICustomApplicationBaseViewModel : IApplicationBaseViewModel
    {
    }

    public interface IOtherCustomApplicationBaseViewModel : IApplicationBaseViewModel
    {
    }
}
