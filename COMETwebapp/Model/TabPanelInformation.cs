// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabPanelInformation.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Model
{
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="TabPanelInformation" /> provides required information related to a panel
    /// </summary>
    public class TabPanelInformation : DisposableObject
    {
        /// <summary>
        /// Backing field for the property <see cref="CurrentTab" />
        /// </summary>
        private TabbedApplicationInformation currentTab;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedApplicationInformation" /> class.
        /// </summary>
        public TabPanelInformation()
        {
            this.Disposables.Add(this.OpenTabs.Connect().WhereReasonsAre(ListChangeReason.Remove, ListChangeReason.RemoveRange).Subscribe(this.OnOpenTabRemoved));
        }

        /// <summary>
        /// Gets or sets the current tab
        /// </summary>
        public TabbedApplicationInformation CurrentTab
        {
            get => this.currentTab;
            set => this.RaiseAndSetIfChanged(ref this.currentTab, value);
        }

        /// <summary>
        /// Gets the collection of all <see cref="TabbedApplicationInformation" /> contained by the panel
        /// </summary>
        public SourceList<TabbedApplicationInformation> OpenTabs { get; } = new();

        /// <summary>
        /// Closes the provided tab, allowing its <see cref="IApplicationBaseViewModel" /> to be disposed
        /// </summary>
        /// <param name="tab">The <see cref="TabbedApplicationInformation" /> to close</param>
        /// <remarks>
        /// Removing a tab from <see cref="OpenTabs" /> without going through this method only moves it out of the panel and
        /// keeps its <see cref="IApplicationBaseViewModel" /> alive, which is required when the tab is moved to another panel
        /// </remarks>
        public void CloseTab(TabbedApplicationInformation tab)
        {
            tab.ApplicationBaseViewModel.IsAllowedToDispose = true;
            this.OpenTabs.Remove(tab);
        }

        /// <summary>
        /// Closes the provided tabs, allowing their <see cref="IApplicationBaseViewModel" /> to be disposed
        /// </summary>
        /// <param name="tabs">The collection of <see cref="TabbedApplicationInformation" /> to close</param>
        public void CloseTabs(IEnumerable<TabbedApplicationInformation> tabs)
        {
            var tabsToClose = tabs.ToList();

            foreach (var tab in tabsToClose)
            {
                tab.ApplicationBaseViewModel.IsAllowedToDispose = true;
            }

            this.OpenTabs.RemoveMany(tabsToClose);
        }

        /// <summary>
        /// Method executed when one or more open tabs are removed
        /// </summary>
        /// <param name="changeSet">The change set containing the removed <see cref="TabbedApplicationInformation" /></param>
        private void OnOpenTabRemoved(IChangeSet<TabbedApplicationInformation> changeSet)
        {
            var removedTabs = changeSet
                .SelectMany(change => change.Range.Count > 0 ? change.Range.ToList() : [change.Item.Current])
                .ToList();

            if (removedTabs.Contains(this.CurrentTab))
            {
                this.CurrentTab = this.OpenTabs.Items.FirstOrDefault();
            }
        }
    }
}
