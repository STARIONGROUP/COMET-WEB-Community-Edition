// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISingleThingApplicationTemplateViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.ViewModels.Components.Applications
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// ViewModel that will englobe all applications where only one <typeparamref name="TThing"/> needs to be selected
    /// </summary>
    /// <typeparam name="TThing">Any <see cref="Thing"/></typeparam>
    public interface ISingleThingApplicationTemplateViewModel<TThing>: IApplicationTemplateViewModel where TThing : Thing
    {
        /// <summary>
        /// Value asserting that the user should select an <typeparamref name="TThing" />
        /// </summary>
        bool IsOnSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the selected <typeparamref name="TThing" />
        /// </summary>
        TThing SelectedThing { get; set; }

        /// <summary>
        /// Gets the <see cref="IThingSelectorViewModel{TThing}" />
        /// </summary>
        IThingSelectorViewModel<TThing> SelectorViewModel { get; }

        /// <summary>
        /// Asks the user to selects the <typeparamref name="TThing"/> that he wants to works with
        /// </summary>
        void AskToSelectThing();

        /// <summary>
        /// Selects a <typeparamref name="TThing" />
        /// </summary>
        /// <param name="thing">The newly selected <typeparamref name="TThing" /></param>
        void OnThingSelect(TThing thing);
    }
}
