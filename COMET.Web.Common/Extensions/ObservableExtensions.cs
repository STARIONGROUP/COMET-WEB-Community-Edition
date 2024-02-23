// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ObservableExtensions.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Extensions
{
    using System.Reactive.Linq;

    /// <summary>
    /// Extension class for the <see cref="IObservable{T}" />
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Subscribe to an <see cref="IObservable{T}" /> with async capabilities
        /// </summary>
        /// <typeparam name="T">An object</typeparam>
        /// <param name="source">The source <see cref="IObservable{T}" /></param>
        /// <param name="onNextAsync">The <see cref="Func{TResult,UResult}" /> to call</param>
        /// <returns>The created <see cref="IDisposable" /></returns>
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNextAsync)
        {
            return source.Select(x => Observable.FromAsync(() => onNextAsync(x))).Concat().Subscribe();
        }
    }
}
