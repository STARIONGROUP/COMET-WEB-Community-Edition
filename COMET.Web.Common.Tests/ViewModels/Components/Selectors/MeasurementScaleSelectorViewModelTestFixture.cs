// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScaleSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Selectors
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MeasurementScaleSelectorViewModelTestFixture
    {
        private MeasurementScaleSelectorViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), Name = "second", ShortName = "s" };
            var rdl = new SiteReferenceDataLibrary { Scale = { scale } };
            var siteDirectory = new SiteDirectory { SiteReferenceDataLibrary = { rdl } };

            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new MeasurementScaleSelectorViewModel(sessionService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySelectedMeasurementScaleChangeInvokesCallback()
        {
            MeasurementScale captured = null;

            // The callback is assigned AFTER construction (as consumers do via an object initializer). Regression:
            // the VM used to subscribe with the method group, capturing the empty callback set at construction time,
            // so a scale change silently did nothing.
            this.viewModel.OnSelectedMeasurementScaleChange = new EventCallbackFactory().Create<MeasurementScale>(this, scale => captured = scale);

            var minutes = new RatioScale { Iid = Guid.NewGuid(), Name = "minute", ShortName = "min" };
            this.viewModel.SelectedMeasurementScale = minutes;

            Assert.That(captured, Is.EqualTo(minutes), "Changing the scale after wiring the callback must invoke it.");
        }
    }
}
