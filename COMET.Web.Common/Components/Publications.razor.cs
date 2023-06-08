// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Publications.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
namespace COMET.Web.Common.Components
{
    using COMET.Web.Common.ViewModels.Components.Publications;
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;

    using DevExpress.Blazor;
    using DevExpress.Blazor.Internal;

    using DynamicData;
    
    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// Support class for the Publications component
    /// </summary>
    public partial class Publications
    {
        /// <summary>
        /// Gets or sets the <see cref="IPublicationsViewModel"/>
        /// </summary>
        [Parameter]
        public IPublicationsViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x=>x.ViewModel.CanPublish)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().Subscribe(_ =>
            {
                this.ViewModel.CanPublish = this.ViewModel.Rows.Items.Any(x=>x.IsSelected);
            }));
        }
        
        /// <summary>
        /// Handler for when a group checkbox state changed
        /// </summary>
        /// <param name="context">the <see cref="GridDataColumnGroupRowTemplateContext"/></param>
        /// <param name="isChecked">true if the checkbox is checked, false otherwise</param>
        private void OnGroupSelectionChanged(GridDataColumnGroupRowTemplateContext context, bool isChecked)
        {
            var rowsFilteredByDomain = this.ViewModel.Rows.Items.Where(x => x.Domain == context.GroupValueDisplayText).ToList();
            rowsFilteredByDomain.ForEach(x=>x.IsSelected = isChecked);
        }

        /// <summary>
        /// Handler for when a row checkbox state changed
        /// </summary>
        /// <param name="rows">the rows asociated to the change</param>
        private void OnRowSelectionChanged(IReadOnlyList<object> rows)
        {
            if (rows is GridSelectedDataItemsCollection dataCollection)
            {
                var selectedRows = dataCollection.SelectedDataItems.Cast<PublicationRowViewModel>();
                var deselectedRows = dataCollection.DeselectedDataItems.Cast<PublicationRowViewModel>();

                selectedRows.ForEach(x=>x.IsSelected = true);
                deselectedRows.ForEach(x=>x.IsSelected = false);
            }
        }
    }
}
