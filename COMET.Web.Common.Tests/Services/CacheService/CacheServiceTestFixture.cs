// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CacheServiceTestFixture.cs" company="Starion Group S.A.">
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


namespace COMET.Web.Common.Tests.Services.CacheService
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.Cache;

    using NUnit.Framework;

    [TestFixture]
    public class CacheServiceTestFixture
    {
        private CacheService cacheService;

        [SetUp]
        public void Setup()
        {
            this.cacheService = new CacheService();
        }

        [Test]
        public void VerifyAddNewKey()
        {
            var engineeringModel = new EngineeringModel();
            var browserSessionSettingKey = BrowserSessionSettingKey.LastUsedEngineeringModel;

            Assert.That(() => this.cacheService.AddOrUpdateBrowserSessionSetting(browserSessionSettingKey, engineeringModel), Throws.Nothing);

            Assert.That(this.cacheService.TryGetBrowserSessionSetting(browserSessionSettingKey, out var result), Is.True);

            Assert.That(result, Is.EqualTo(engineeringModel));
        }

        [Test]
        public void VerifyKeyDoesNotExistWorksAsExpected()
        {
            var browserSessionSettingKey = BrowserSessionSettingKey.LastUsedEngineeringModel;

            Assert.That(this.cacheService.TryGetBrowserSessionSetting(browserSessionSettingKey, out var result), Is.False);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void VerifyOverwriteExistingKey()
        {
            var engineeringModel1 = new EngineeringModel();
            var browserSessionSettingKey = BrowserSessionSettingKey.LastUsedEngineeringModel;

            Assert.That(() => this.cacheService.AddOrUpdateBrowserSessionSetting(browserSessionSettingKey, engineeringModel1), Throws.Nothing);

            Assert.That(this.cacheService.TryGetBrowserSessionSetting(browserSessionSettingKey, out var result), Is.True);

            Assert.That(result, Is.EqualTo(engineeringModel1));

            var engineeringModel2 = new EngineeringModel();

            Assert.That(() => this.cacheService.AddOrUpdateBrowserSessionSetting(browserSessionSettingKey, engineeringModel2), Throws.Nothing);

            Assert.That(this.cacheService.TryGetBrowserSessionSetting(browserSessionSettingKey, out var result2), Is.True);

            Assert.That(result2, Is.EqualTo(engineeringModel2));
        }

        [Test]
        public void VerifyGetOrAddKey()
        {
            var engineeringModel1 = new EngineeringModel();
            var browserSessionSettingKey = BrowserSessionSettingKey.LastUsedEngineeringModel;

            Assert.That(this.cacheService.TryGetOrAddBrowserSessionSetting(browserSessionSettingKey, engineeringModel1, out var result), Is.True);

            Assert.That(result, Is.EqualTo(engineeringModel1));

            Assert.That(this.cacheService.TryGetBrowserSessionSetting(browserSessionSettingKey, out var result2), Is.True);

            Assert.That(result2, Is.EqualTo(engineeringModel1));

            var engineeringModel2 = new EngineeringModel();

            Assert.That(this.cacheService.TryGetOrAddBrowserSessionSetting(browserSessionSettingKey, engineeringModel2, out var result3), Is.True);

            Assert.That(result3, Is.EqualTo(engineeringModel1));

            Assert.That(this.cacheService.TryGetBrowserSessionSetting(browserSessionSettingKey, out var result4), Is.True);

            Assert.That(result4, Is.EqualTo(engineeringModel1));
        }
    }
}
