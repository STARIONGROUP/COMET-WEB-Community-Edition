// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValidationMessageComponentTestFixture.cs" company="RHEA System S.A.">
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

namespace UI_DSM.Client.Tests.Components
{
    using Bunit;
    
    using COMETwebapp.Components.Shared;
    using COMETwebapp.ViewModels.Components.Shared;
    
    using DynamicData;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ValidationMessageComponentTestFixture
    {
        private TestContext context;
        private IValidationMessageViewModel validationMessageViewModel;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.validationMessageViewModel = new ValidationMessageViewModel();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.Dispose();
        }

        [Test]
        public void VerifyRenderErrors()
        {
            this.validationMessageViewModel.Messages.Add("validation message test");

            var renderer = this.context.RenderComponent<ValidationMessageComponent>(parameters => parameters
                .Add(p => p.ViewModel, this.validationMessageViewModel));

            Assert.That(renderer.FindAll("li").Count, Is.EqualTo(1));
        }
    }
}
