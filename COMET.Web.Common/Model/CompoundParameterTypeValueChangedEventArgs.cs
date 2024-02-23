// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeValueChangedEventArgs.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Model
{
    /// <summary>
    /// Class for the arguments of the event raised when a value in a
    /// <see cref="CDP4Common.SiteDirectoryData.CompoundParameterType" /> has changed
    /// </summary>
    public class CompoundParameterTypeValueChangedEventArgs
    {
        /// <summary>
        /// Creates a new instance of type <see cref="CompoundParameterTypeValueChangedEventArgs" />
        /// </summary>
        /// <param name="index">the index of the value changed in the value set</param>
        /// <param name="value">the new value</param>
        public CompoundParameterTypeValueChangedEventArgs(int index, string value)
        {
            this.Index = index;
            this.Value = value;
        }

        /// <summary>
        /// Gets the index of the value changed in the value sets
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the new value of the value set for the corresponding <see cref="Index" />
        /// </summary>
        public string Value { get; }
    }
}
