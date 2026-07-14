// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterValueFormatterTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Utilities
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Utilities;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterValueFormatterTestFixture
    {
        private static SampledFunctionParameterType CreateThreeColumnSampledFunction()
        {
            var quantityKind = new SimpleQuantityKind();
            var sampledFunctionParameterType = new SampledFunctionParameterType();
            sampledFunctionParameterType.IndependentParameterType.Add(new IndependentParameterTypeAssignment { ParameterType = quantityKind });
            sampledFunctionParameterType.DependentParameterType.Add(new DependentParameterTypeAssignment { ParameterType = quantityKind });
            sampledFunctionParameterType.DependentParameterType.Add(new DependentParameterTypeAssignment { ParameterType = quantityKind });

            return sampledFunctionParameterType;
        }

        [Test]
        public void VerifyFormat()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ParameterValueFormatter.Format(["42"]), Is.EqualTo("42"));
                Assert.That(ParameterValueFormatter.Format(["1", "2", "3"]), Is.EqualTo("{1, 2, 3}"));
                Assert.That(ParameterValueFormatter.Format(["42"], new RatioScale { ShortName = "kg" }), Is.EqualTo("42 [kg]"));
                Assert.That(ParameterValueFormatter.ValueAt(new ValueArray<string>(["a", "b"]), 1), Is.EqualTo("b"));
                Assert.That(ParameterValueFormatter.ValueAt(new ValueArray<string>(["a", "b"]), 5), Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void VerifySampledFunctionGroupsByRow()
        {
            var sampledFunctionParameterType = CreateThreeColumnSampledFunction();

            Assert.Multiple(() =>
            {
                Assert.That(ParameterValueFormatter.Format(new ValueArray<string>(["3", "4", "1", "4", "8", "2"]), sampledFunctionParameterType),
                    Is.EqualTo("{3, 4, 1}, {4, 8, 2}"), "A 2x3 sampled function must group each row so the table shape is legible.");

                Assert.That(ParameterValueFormatter.Format(new ValueArray<string>(["-", "-", "-"]), sampledFunctionParameterType),
                    Is.EqualTo("{-, -, -}"), "A single sampled row is still braced.");

                // Any other parameter type falls back to the flat formatting.
                Assert.That(ParameterValueFormatter.Format(new ValueArray<string>(["1", "2", "3"]), new SimpleQuantityKind()),
                    Is.EqualTo("{1, 2, 3}"));
            });
        }
    }
}
