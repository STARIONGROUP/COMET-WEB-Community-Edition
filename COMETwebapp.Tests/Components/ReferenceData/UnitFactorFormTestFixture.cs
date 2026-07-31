// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="UnitFactorFormTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;
    using COMETwebapp.Components.ReferenceData.MeasurementUnits;

    using Microsoft.AspNetCore.Components.Forms;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for the <see cref="UnitFactorForm" /> component.
    /// </summary>
    [TestFixture]
    public class UnitFactorFormTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<UnitFactorForm> renderer;
        private UnitFactor item;
        private bool isSaved;
        private bool isCanceled;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.item = new UnitFactor
            {
                Iid = Guid.NewGuid(),
                Unit = new SimpleUnit { ShortName = "m" },
                Exponent = "2"
            };

            this.isSaved = false;
            this.isCanceled = false;

            var measurementUnits = new List<MeasurementUnit>
            {
                this.item.Unit
            };

            this.renderer = this.context.Render<UnitFactorForm>(parameters => parameters
                .Add(p => p.Item, this.item)
                .Add(p => p.MeasurementUnits, measurementUnits)
                .Add(p => p.OnSaved, () => Task.FromResult(this.isSaved = true))
                .Add(p => p.OnCanceled, () => Task.FromResult(this.isCanceled = true)));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyFormSubmissionAndCancellation()
        {
            var editForm = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.That(this.isSaved, Is.True);

            var formButtons = this.renderer.FindComponent<FormButtons>();
            await this.renderer.InvokeAsync(formButtons.Instance.OnCancel.InvokeAsync);

            Assert.That(this.isCanceled, Is.True);
        }

        [Test]
        public void VerifyUnitFactorFormRendering()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.Item, Is.SameAs(this.item));
                Assert.That(this.renderer.Instance.MeasurementUnits.ToList(), Has.Count.EqualTo(1));
            }
        }
    }
}
