// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MockedLoggerHelper.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Test.Helpers
{
    using System.Linq.Expressions;

    using Microsoft.Extensions.Logging;

    using Moq;

    /// <summary>
    /// Helper class for any <see cref="Moq.Mock"/> for <seealso cref="ILogger{TCategoryName}"/>
    /// </summary>
    public static class MockedLoggerHelper
    {
        /// <summary>
        /// Provides the matching expression to verify
        /// </summary>
        /// <typeparam name="T">The type for the <see cref="ILogger{TCategoryName}"/></typeparam>
        /// <param name="level">The <see cref="LogLevel"/></param>
        /// <param name="predicate">A predicate for the matching log message</param>
        /// <returns>An <see cref="Expression"/></returns>
        private static Expression<Action<ILogger<T>>> Verify<T>(LogLevel level, Func<object, bool> predicate)
        {
            return x => x.Log(level, 0, It.Is<It.IsAnyType>((o, t) => predicate(o)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>());
        }

        /// <summary>
        /// Verifies that the <see cref="ILogger{TCategoryName}"/> has log message that matches the provided <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">The type for the <see cref="ILogger{TCategoryName}"/></typeparam>
        /// <param name="mock">The <see cref="Mock"/> <see cref="ILogger{TCategoryName}"/></param>
        /// <param name="level">The <see cref="LogLevel"/></param>
        /// <param name="predicate">A predicate for the matching log message</param>
        /// <param name="times">The <see cref="DevExpress.Pdf.Native.BouncyCastle.Utilities.Times"/> to verify</param>
        public static void Verify<T>(this Mock<ILogger<T>> mock, LogLevel level, Func<object, bool> predicate, Times times)
        {
            mock.Verify(Verify<T>(level, predicate), times);
        }
    }
}
