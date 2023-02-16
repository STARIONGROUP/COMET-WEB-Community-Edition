// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemTreeTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.SystemRepresentation
{
    using System.Collections.Generic;

    using BlazorStrap;

    using Bunit;
    
    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    
    using Microsoft.Extensions.DependencyInjection;
    
    using NUnit.Framework;
        
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SystemTreeTestFixture
    {
        private TestContext context;
        private ISystemTreeViewModel systemTreeViewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddBlazorStrap();
            this.context.Services.AddAntDesign();
            this.context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            this.systemTreeViewModel = new SystemTreeViewModel()
            {
                SystemNodes = new List<SystemNode>()
            };
        }

        [Test]
        public void VerifyComponent()
        {
            var renderer = this.context.RenderComponent<SystemTree>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.systemTreeViewModel);
            });

            Assert.That(renderer.Instance, Is.Not.Null);

            this.systemTreeViewModel.SystemNodes = new List<SystemNode> { new SystemNode("node1") };
            
            renderer.Render();

            Assert.That(renderer.Markup, Does.Contain("node1"));
        }
    }
}