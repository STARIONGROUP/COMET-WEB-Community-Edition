// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabPanelInformationTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Model
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Model;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabPanelInformationTestFixture
    {
        private TabPanelInformation panel;
        private Mock<IApplicationBaseViewModel> applicationBaseViewModel;
        private TabbedApplicationInformation tab;

        [SetUp]
        public void SetUp()
        {
            this.applicationBaseViewModel = new Mock<IApplicationBaseViewModel>();
            this.applicationBaseViewModel.SetupProperty(x => x.IsAllowedToDispose, false);

            var engineeringModelSetup = new EngineeringModelSetup();

            var iteration = new Iteration
            {
                IterationSetup = new IterationSetup
                {
                    Container = engineeringModelSetup
                },
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = engineeringModelSetup
                }
            };

            this.tab = new TabbedApplicationInformation(this.applicationBaseViewModel.Object, typeof(EngineeringModelBody), iteration);

            this.panel = new TabPanelInformation();
            this.panel.OpenTabs.Add(this.tab);
            this.panel.CurrentTab = this.tab;
        }

        [Test]
        public void VerifyCloseTab()
        {
            this.panel.CloseTab(this.tab);

            Assert.Multiple(() =>
            {
                Assert.That(this.applicationBaseViewModel.Object.IsAllowedToDispose, Is.True);
                Assert.That(this.panel.OpenTabs.Items, Is.Empty);
                Assert.That(this.panel.CurrentTab, Is.Null);
            });

            var otherViewModel = new Mock<IApplicationBaseViewModel>();
            otherViewModel.SetupProperty(x => x.IsAllowedToDispose, false);
            var remainingTab = new TabbedApplicationInformation(otherViewModel.Object, typeof(EngineeringModelBody), null);
            var tabToClose = new TabbedApplicationInformation(this.applicationBaseViewModel.Object, typeof(EngineeringModelBody), null);

            this.panel.OpenTabs.Add(remainingTab);
            this.panel.OpenTabs.Add(tabToClose);
            this.panel.CurrentTab = tabToClose;

            this.panel.CloseTab(tabToClose);

            Assert.That(this.panel.CurrentTab, Is.EqualTo(remainingTab));
        }

        [Test]
        public void VerifyCloseTabs()
        {
            var otherViewModel = new Mock<IApplicationBaseViewModel>();
            otherViewModel.SetupProperty(x => x.IsAllowedToDispose, false);

            var remainingViewModel = new Mock<IApplicationBaseViewModel>();
            remainingViewModel.SetupProperty(x => x.IsAllowedToDispose, false);

            var otherTab = new TabbedApplicationInformation(otherViewModel.Object, typeof(EngineeringModelBody), null);
            var remainingTab = new TabbedApplicationInformation(remainingViewModel.Object, typeof(EngineeringModelBody), null);

            this.panel.OpenTabs.Add(otherTab);
            this.panel.OpenTabs.Add(remainingTab);
            this.panel.CurrentTab = this.tab;

            this.panel.CloseTabs([this.tab, otherTab]);

            Assert.Multiple(() =>
            {
                Assert.That(this.applicationBaseViewModel.Object.IsAllowedToDispose, Is.True);
                Assert.That(otherViewModel.Object.IsAllowedToDispose, Is.True);
                Assert.That(remainingViewModel.Object.IsAllowedToDispose, Is.False);
                Assert.That(this.panel.OpenTabs.Items, Has.Exactly(1).Items);
                Assert.That(this.panel.OpenTabs.Items, Does.Contain(remainingTab));
                Assert.That(this.panel.CurrentTab, Is.EqualTo(remainingTab));
            });
        }

        [Test]
        public void VerifyOpenTabsRemoveDoesNotAllowDisposal()
        {
            this.panel.OpenTabs.Remove(this.tab);

            Assert.Multiple(() =>
            {
                Assert.That(this.panel.OpenTabs.Items, Is.Empty);
                Assert.That(this.panel.CurrentTab, Is.Null);
                Assert.That(this.applicationBaseViewModel.Object.IsAllowedToDispose, Is.False);
            });
        }
    }
}
