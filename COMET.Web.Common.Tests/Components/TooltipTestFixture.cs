// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TooltipTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using COMET.Web.Common.Components;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class TooltipTestFixture
    {
        [Test]
        public void VerifyTooltipComponent()
        {
            var context = new TestContext();
            const string cssClass = "cssclass";
            const string text = "Some text";

            var renderer = context.RenderComponent<Tooltip>(parameters =>
            {
                parameters.Add(p => p.Text, text);
                parameters.Add(p => p.MarginBottom, cssClass);

                parameters.Add(p => p.ChildContent, builder =>
                {
                    builder.OpenElement(0, "p");
                    builder.AddContent(1, "body"); 
                    builder.CloseElement();
                });
            });

            var span = renderer.Find("span");
            
            Assert.Multiple(() =>
            {
                Assert.That(span.ClassName,Does.Contain(cssClass));
                Assert.That(span.TextContent, Is.EqualTo(text));
            });

            var content = renderer.Find("p");
            Assert.That(content.TextContent, Is.EqualTo("body"));

            context.Dispose();
        }
    }
}
