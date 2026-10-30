// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVImeCompatibilityTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Utilities.VandV
{
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Utilities.VandV;

    using NUnit.Framework;

    /// <summary>
    /// Guards compatibility with the COMET IME. The IME's V&amp;V dialogs store an enumeration value by its name
    /// (Non-Compliant), the stock parameter value editor by its short name (Non_Compliant). Two items holding the same
    /// values in the two spellings must read identically everywhere, otherwise a model edited in both clients silently
    /// changes meaning.
    /// </summary>
    [TestFixture]
    public class VandVImeCompatibilityTestFixture
    {
        private static readonly (string ShortName, string Name, string StoredShortName)[] SharedValues =
        [
            (VandVConstants.Method, "Review of Design", "Review_of_Design"),
            (VandVConstants.Stage, "In-Service", "In_Service"),
            (VandVConstants.Level, "System-of-Systems", "System_of_Systems"),
            (VandVConstants.Criticality, "Mission-critical", "Mission_critical"),
            (VandVConstants.Status, "Not Applicable", "Not_Applicable"),
            (VandVConstants.Compliance, "Partially Compliant", "Partially_Compliant"),
            (VandVConstants.Closure, "Further V&V Required", "Further_VV_Required")
        ];

        [Test]
        public void VerifyBothSpellingsReadIdentically()
        {
            var data = new VandVTestData();
            var specification = data.AddSpecification("SPEC");
            var byName = data.AddRequirement(specification, "R1", VandVTestData.RequirementCategoryShortName);
            var byShortName = data.AddRequirement(specification, "R2", VandVTestData.RequirementCategoryShortName);
            var vandVSpecification = data.AddSpecification(VandVConstants.VandVSpecificationShortName);
            var imeItem = data.AddRequirement(vandVSpecification, "VNV_R1_1", VandVConstants.VnVItemCategory);
            var stockItem = data.AddRequirement(vandVSpecification, "VNV_R2_1", VandVConstants.VnVItemCategory);
            data.Link(imeItem, byName, VandVConstants.VerifiesCategory);
            data.Link(stockItem, byShortName, VandVConstants.VerifiesCategory);

            foreach (var (shortName, name, storedShortName) in SharedValues)
            {
                data.SetValue(imeItem, shortName, name);
                data.SetValue(stockItem, shortName, storedShortName);
            }

            data.SetValue(imeItem, VandVConstants.Closed, "true");
            data.SetValue(stockItem, VandVConstants.Closed, "True");
            data.Annotate<RequestForWaiver>(imeItem, AnnotationStatusKind.CLOSED);
            data.Annotate<RequestForWaiver>(stockItem, AnnotationStatusKind.CLOSED);

            var model = data.BuildModel();

            Assert.Multiple(() =>
            {
                foreach (var (shortName, name, _) in SharedValues)
                {
                    Assert.That(VandVQueries.GetDisplayValue(imeItem, shortName), Is.EqualTo(name), shortName);
                    Assert.That(VandVQueries.GetDisplayValue(stockItem, shortName), Is.EqualTo(name), shortName);
                }

                Assert.That(VandVQueries.IsShortfall(VandVQueries.GetCompliance(stockItem)), Is.True);
                Assert.That(VandVQueries.IsClosed(stockItem), Is.True);
                Assert.That(VandVQueries.GetItemOutcome(model, stockItem), Is.EqualTo(VandVQueries.GetItemOutcome(model, imeItem)));
                Assert.That(VandVQueries.GetItemOutcome(model, stockItem), Is.EqualTo(VandVItemOutcome.Passed), "closed with an accepted waiver");
                Assert.That(model.StageGates, Is.EqualTo(VandVConstants.DefaultStageGates), "both spellings match the defined In-Service gate");

                Assert.That(VandVQueries.FormatRequirementCoverage(VandVQueries.GetRollUp(model, model.GetCoveringItems(byShortName))),
                    Is.EqualTo(VandVQueries.FormatRequirementCoverage(VandVQueries.GetRollUp(model, model.GetCoveringItems(byName)))));
            });
        }

        [Test]
        public void VerifySeededValueShortNamesMatchTheIme()
        {
            var expected = new Dictionary<string, string>
            {
                ["Review of Design"] = "Review_of_Design",
                ["In-Service"] = "In_Service",
                ["System-of-Systems"] = "System_of_Systems",
                ["Mission-critical"] = "Mission_critical",
                ["In Progress"] = "In_Progress",
                ["Non-Compliant"] = "Non_Compliant",
                ["Closes Out Requirement"] = "Closes_Out_Requirement",
                ["Further V&V Required"] = "Further_VV_Required",
                ["Not Run"] = "Not_Run"
            };

            var data = new VandVTestData();

            var seeded = data.ParameterTypes.Values
                .OfType<EnumerationParameterType>()
                .SelectMany(x => x.ValueDefinition)
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => x.First().ShortName);

            Assert.Multiple(() =>
            {
                foreach (var (name, shortName) in expected)
                {
                    Assert.That(seeded[name], Is.EqualTo(shortName), name);
                }

                Assert.That(VandVManifest.ParameterTypes.Single(x => x.ShortName == VandVConstants.Stage).Values, Is.EqualTo(VandVConstants.DefaultStageGates));
            });
        }
    }
}
