﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelMenuRow.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that handle an open <see cref="Iteration" /> into the <see cref="ModelMenu" />
    /// </summary>
    public partial class ModelMenuRow
    {
        /// <summary>
        /// The <see cref="ModelMenuRowViewModel" />
        /// </summary>
        [Parameter]
        public ModelMenuRowViewModel ViewModel { get; set; }

        /// <summary>
        /// The current index of the <see cref="ModelMenuRow" />
        /// </summary>
        [Parameter]
        public int RowIndex { get; set; }

        /// <summary>
        /// The unique id of the <see cref="ModelMenuRow" />
        /// </summary>
        private string RowId => $"model-entry-row-{this.RowIndex}";

        /// <summary>
        /// The unique id of the close model row
        /// </summary>
        private string CloseModelId => $"{this.RowId}-close";

        /// <summary>
        /// The unique id of the switch domain model row
        /// </summary>
        private string SwitchModelId => $"{this.RowId}-switch";
    }
}
