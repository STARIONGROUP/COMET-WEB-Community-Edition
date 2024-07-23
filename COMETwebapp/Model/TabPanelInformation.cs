// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabPanelInformation.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
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

namespace COMETwebapp.Model
{
    using COMET.Web.Common.Utilities.DisposableObject;

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
        public SourceList<TabbedApplicationInformation> OpenTabs { get; set; } = new();

        /// <summary>
        /// Method executed when one or more open tabs are removed
        /// </summary>
        /// <param name="changeSet">The change set containing the removed <see cref="TabbedApplicationInformation" /></param>
        private void OnOpenTabRemoved(IChangeSet<TabbedApplicationInformation> changeSet)
        {
            foreach (var result in changeSet.ToList())
            {
                if (result.Range.Count > 0)
                {
                    foreach (var tabToRemove in result.Range)
                    {
                        tabToRemove.ApplicationBaseViewModel.IsAllowedToDispose = true;
                    }
                }
                else
                {
                    result.Item.Current.ApplicationBaseViewModel.IsAllowedToDispose = true;
                }
            }

            var wasCurrentTabRemoved = changeSet
                .Select(x => x.Item.Current)
                .Contains(this.CurrentTab);

            if (wasCurrentTabRemoved)
            {
                this.CurrentTab = this.OpenTabs.Items.FirstOrDefault();
            }
        }
    }
}
