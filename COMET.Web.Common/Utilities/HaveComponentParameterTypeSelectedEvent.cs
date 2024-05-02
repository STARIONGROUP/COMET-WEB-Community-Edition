﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveComponentParameterTypeSelectedEvent.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Utilities
{
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

	/// <summary>
	/// Class used to notify an observer that the <see cref="HaveComponentParameterTypeSelectedEvent" /> is selected.
	/// </summary>
	public class HaveComponentParameterTypeSelectedEvent
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="HaveComponentParameterTypeSelectedEvent" /> class.
		/// </summary>
		/// <param name="haveComponentParameter">The <see cref="IHaveComponentParameterTypeEditor" /></param>
		public HaveComponentParameterTypeSelectedEvent(IHaveComponentParameterTypeEditor haveComponentParameter)
        {
            this.HaveComponentParameter = haveComponentParameter;
        }

		/// <summary>
		/// Gets the <see cref="IHaveComponentParameterTypeEditor" />
		/// </summary>
		public IHaveComponentParameterTypeEditor HaveComponentParameter { get; }
    }
}
