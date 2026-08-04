// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometButtonStyleExtensionsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Extensions
{
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;

    using DevExpress.Blazor;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CometButtonStyleExtensions" /> class.
    /// </summary>
    [TestFixture]
    public class CometButtonStyleExtensionsTestFixture
    {
        [Test]
        public void VerifyToButtonRenderStyle()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(CometButtonStyle.Primary.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Primary));
                Assert.That(CometButtonStyle.Secondary.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Secondary));
                Assert.That(CometButtonStyle.Danger.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Danger));
                Assert.That(CometButtonStyle.Success.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Success));
                Assert.That(CometButtonStyle.Warning.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Warning));
                Assert.That(CometButtonStyle.Info.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Info));
                Assert.That(CometButtonStyle.Light.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Light));
                Assert.That(CometButtonStyle.Dark.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Dark));
                Assert.That(CometButtonStyle.Link.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Link));
                Assert.That(CometButtonStyle.Edit.ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Info));
                Assert.That(((CometButtonStyle)99).ToButtonRenderStyle(), Is.EqualTo(ButtonRenderStyle.Primary));
            }
        }
    }
}
