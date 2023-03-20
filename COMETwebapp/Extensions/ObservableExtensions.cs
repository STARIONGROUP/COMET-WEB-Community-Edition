// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ObservableExtensions.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Extensions
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
