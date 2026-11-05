// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVManifestTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Utilities.VandV;

    using NUnit.Framework;

    [TestFixture]
    public class VandVManifestTestFixture
    {
        private static readonly string[] ActivityReleaseShortNames = [VandVConstants.VnVActivityCategory, VandVConstants.PerformedByCategory, VandVConstants.VnVReportCategory];

        [Test]
        public void VerifyParameterTypesCategoriesAndRules()
        {
            var parameterTypes = VandVManifest.ParameterTypes.Select(x => x.ShortName).ToList();
            var categories = VandVManifest.Categories.Select(x => x.ShortName).ToList();

            var ruleReferences = VandVManifest.ParameterizedCategoryRules.SelectMany(x => x.ParameterTypeShortNames)
                .Concat(VandVConstants.DerivedFromActivity)
                .Distinct()
                .ToList();

            var categoryReferences = VandVManifest.ParameterizedCategoryRules.Select(x => x.CategoryShortName)
                .Concat(VandVManifest.BinaryRelationshipRules.SelectMany(x => new[] { x.RelationshipCategoryShortName, x.SourceCategoryShortName }))
                .Concat(VandVManifest.Categories.Where(x => x.SuperCategoryShortName != null).Select(x => x.SuperCategoryShortName))
                .Concat(VandVConstants.RequiredForItemWrite)
                .Distinct()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(parameterTypes, Is.Unique);
                Assert.That(categories, Is.Unique);
                Assert.That(parameterTypes, Has.Count.EqualTo(30));
                Assert.That(categories, Has.Count.EqualTo(15));
                Assert.That(ruleReferences, Is.SubsetOf(parameterTypes));
                Assert.That(categoryReferences, Is.SubsetOf(categories));

                foreach (var enumeration in VandVManifest.ParameterTypes.Where(x => x.Kind == VandVParameterKind.Enumeration))
                {
                    Assert.That(enumeration.Values, Is.Not.Empty, enumeration.ShortName);
                    Assert.That(enumeration.Values.Select(VandVConstants.ToShortName), Is.Unique, enumeration.ShortName);
                }

                Assert.That(VandVManifest.ParameterTypes.Where(x => x.Kind != VandVParameterKind.Enumeration).SelectMany(x => x.Values), Is.Empty);
            });
        }

        [Test]
        public void VerifyCheck()
        {
            var empty = VandVManifest.Check(null);
            var complete = VandVManifest.Check([new VandVTestData().Library]);

            var olderRelease = new VandVTestData();
            olderRelease.Library.DefinedCategory.RemoveAll(x => ActivityReleaseShortNames.Contains(x.ShortName));
            olderRelease.Library.ParameterType.RemoveAll(x => x.ShortName == VandVConstants.Closure);
            var partial = VandVManifest.Check([olderRelease.Library]);

            var split = new VandVTestData();
            var siteLibrary = new SiteReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "SRDL" };
            siteLibrary.ParameterType.AddRange(split.Library.ParameterType.ToList());
            var chained = VandVManifest.Check([split.Library, siteLibrary]);

            var noRequirementCategory = new VandVTestData();
            noRequirementCategory.Library.DefinedCategory.RemoveAll(x => x.ShortName == VandVTestData.RequirementCategoryShortName);
            noRequirementCategory.Library.Rule.Clear();
            var withoutTarget = VandVManifest.Check([noRequirementCategory.Library]);

            Assert.Multiple(() =>
            {
                Assert.That(empty.MissingParameterTypes, Has.Count.EqualTo(VandVManifest.ParameterTypes.Count));
                Assert.That(empty.MissingCategories, Has.Count.EqualTo(VandVManifest.Categories.Count));
                Assert.That(empty.MissingParameterizedCategoryRules, Has.Count.EqualTo(VandVManifest.ParameterizedCategoryRules.Count));
                Assert.That(empty.MissingBinaryRelationshipRules, Has.Count.EqualTo(VandVManifest.BinaryRelationshipRules.Count));
                Assert.That(empty.RequirementCategory, Is.Null);
                Assert.That(empty.IsComplete, Is.False);
                Assert.That(empty.CanWriteItems, Is.False);

                Assert.That(complete.MissingParameterTypes, Is.Empty);
                Assert.That(complete.MissingCategories, Is.Empty);
                Assert.That(complete.RequirementCategory.ShortName, Is.EqualTo(VandVTestData.RequirementCategoryShortName));
                Assert.That(complete.CanWriteItems, Is.True);
                Assert.That(complete.IsComplete, Is.False, "the test library holds no rules");

                Assert.That(partial.MissingCategories.Select(x => x.ShortName), Is.EquivalentTo(ActivityReleaseShortNames));
                Assert.That(partial.MissingParameterTypes.Select(x => x.ShortName), Is.EquivalentTo(new[] { VandVConstants.Closure }));
                Assert.That(partial.CanWriteItems, Is.False);

                Assert.That(chained.MissingParameterTypes, Is.Empty, "a type defined in a required library is reused");

                Assert.That(withoutTarget.RequirementCategory, Is.Null);
                Assert.That(withoutTarget.MissingBinaryRelationshipRules, Has.Count.EqualTo(VandVManifest.BinaryRelationshipRules.Count));
                Assert.That(withoutTarget.MissingParameterizedCategoryRules, Has.Count.EqualTo(VandVManifest.ParameterizedCategoryRules.Count));
            });

            noRequirementCategory.Library.Rule.AddRange(VandVManifest.ParameterizedCategoryRules.Select(x => new ParameterizedCategoryRule { Iid = Guid.NewGuid(), ShortName = x.ShortName }));

            Assert.That(VandVManifest.Check([noRequirementCategory.Library]).IsComplete, Is.True, "binary rules cannot be created without a requirement category, so they do not count");
        }

        [Test]
        public void VerifyResolveRequirementCategory()
        {
            var data = new VandVTestData();
            var requirementCategory = data.CreateCategory("requirement", "Something", [ClassKind.Requirement]);
            data.Library.DefinedCategory.Add(requirementCategory);

            var byName = new VandVTestData();
            byName.Library.DefinedCategory.RemoveAll(x => x.ShortName == VandVTestData.RequirementCategoryShortName);
            var named = byName.CreateCategory("RQ", "Requirement", [ClassKind.Requirement]);
            byName.Library.DefinedCategory.Add(named);

            var unusable = new VandVTestData();
            unusable.Categories[VandVTestData.RequirementCategoryShortName].IsDeprecated = true;
            unusable.Library.DefinedCategory.Add(unusable.CreateCategory("REQUIREMENT", "Requirement", [ClassKind.ElementDefinition]));

            Assert.Multiple(() =>
            {
                Assert.That(VandVManifest.ResolveRequirementCategory(null), Is.Null);
                Assert.That(VandVManifest.ResolveRequirementCategory([data.Library]), Is.SameAs(requirementCategory), "REQUIREMENT wins over REQ, case-insensitive");
                Assert.That(VandVManifest.ResolveRequirementCategory([new VandVTestData().Library]).ShortName, Is.EqualTo(VandVTestData.RequirementCategoryShortName));
                Assert.That(VandVManifest.ResolveRequirementCategory([byName.Library]), Is.SameAs(named));
                Assert.That(VandVManifest.ResolveRequirementCategory([unusable.Library]), Is.Null, "deprecated or not permissible on requirements");
            });
        }
    }
}
