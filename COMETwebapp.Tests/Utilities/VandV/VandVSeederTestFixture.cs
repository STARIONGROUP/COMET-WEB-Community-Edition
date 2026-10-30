// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVSeederTestFixture.cs" company="Starion Group S.A.">
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
    public class VandVSeederTestFixture
    {
        private static readonly string[] ProjectGates = ["MDR", "PDR", "CDR"];

        private SiteReferenceDataLibrary siteLibrary;
        private ModelReferenceDataLibrary modelLibrary;
        private Category requirementCategory;

        [SetUp]
        public void SetUp()
        {
            this.requirementCategory = new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement", PermissibleClass = { ClassKind.Requirement } };
            this.siteLibrary = new SiteReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "SRDL", Name = "Site library" };
            this.siteLibrary.DefinedCategory.Add(this.requirementCategory);
            this.modelLibrary = new ModelReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "MRDL", Name = "Model library", RequiredRdl = this.siteLibrary };
        }

        [Test]
        public void VerifyGetChain()
        {
            var looping = new SiteReferenceDataLibrary { Iid = Guid.NewGuid() };
            looping.RequiredRdl = looping;

            Assert.Multiple(() =>
            {
                Assert.That(VandVSeeder.GetChain(this.modelLibrary), Is.EqualTo(new ReferenceDataLibrary[] { this.modelLibrary, this.siteLibrary }));
                Assert.That(VandVSeeder.GetChain(null), Is.Empty);
                Assert.That(VandVSeeder.GetChain(looping), Has.Count.EqualTo(1), "a library requiring itself does not loop");
            });
        }

        [Test]
        public void VerifyParseStageGates()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVSeeder.ParseStageGates([" PDR ", "", null, "  ", "CDR"]), Is.EqualTo(new[] { "PDR", "CDR" }));
                Assert.That(VandVSeeder.ParseStageGates(null), Is.Empty);
            });
        }

        [Test]
        public void VerifyGetStageGateError()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVSeeder.GetStageGateError(ProjectGates), Is.Null);
                Assert.That(VandVSeeder.GetStageGateError(["", " "]), Is.EqualTo("Define at least one stage gate."));
                Assert.That(VandVSeeder.GetStageGateError(["PDR", "!!"]), Does.Contain("'!!'").And.Contain("letter or digit"));
                Assert.That(VandVSeeder.GetStageGateError(["---"]), Does.Contain("letter or digit"), "separators alone make no short name");
                Assert.That(VandVSeeder.GetStageGateError(["1st review"]), Does.Contain("'1st review' is not a valid name"), "the SDK rules the IME applies: a name starts with a letter");
                Assert.That(VandVSeeder.GetStageGateError(["(CDR)"]), Does.Contain("not a valid name"));
                Assert.That(VandVSeeder.GetStageGateError(VandVConstants.DefaultStageGates), Is.Null, "In-Service is valid");
                Assert.That(VandVSeeder.GetStageGateError(["In Service", "PDR", "in-service"]), Does.Contain("'In Service'").And.Contain("more than once"), "the same short name");
            });
        }

        [Test]
        public void VerifyDescribeMissing()
        {
            var lines = VandVSeeder.DescribeMissing(VandVManifest.Check([this.modelLibrary, this.siteLibrary]));
            var withoutTarget = VandVSeeder.DescribeMissing(VandVManifest.Check([this.modelLibrary]));

            Assert.Multiple(() =>
            {
                Assert.That(lines, Has.Count.EqualTo(VandVManifest.ParameterTypes.Count + VandVManifest.Categories.Count + 3 + 2));
                Assert.That(lines, Does.Contain("Parameter type vnv_method (V&V Method): enumeration of Inspection, Analysis, Similarity, Demonstration, Test, Review of Design"));
                Assert.That(lines, Does.Contain("Parameter type vnv_stage (V&V Stage Gate): enumeration of the stage gates defined below"));
                Assert.That(lines, Does.Contain("Parameter type vnv_planned_date (V&V Planned Date): date"));
                Assert.That(lines, Does.Contain("Category VnVReport (VnV Report) on RequirementsSpecification"));
                Assert.That(lines, Does.Contain("Rule VnVItemAttributesRule (V&V Item mandatory attributes)"));
                Assert.That(lines, Does.Contain("Rule verifiesRule (VnVItem verifies REQ)"));
                Assert.That(withoutTarget.Any(x => x.StartsWith("Rule verifiesRule")), Is.False, "binary rules cannot be created without a requirement category");
            });
        }

        [Test]
        public void VerifyGetRequiredClassKinds()
        {
            var everything = VandVSeeder.GetRequiredClassKinds(VandVManifest.Check([this.modelLibrary, this.siteLibrary]));
            var nothing = VandVSeeder.GetRequiredClassKinds(VandVManifest.Check([new VandVTestData().Library]));

            Assert.Multiple(() =>
            {
                Assert.That(everything, Is.EquivalentTo(new[]
                {
                    ClassKind.EnumerationParameterType, ClassKind.TextParameterType, ClassKind.DateParameterType,
                    ClassKind.BooleanParameterType, ClassKind.Category, ClassKind.ParameterizedCategoryRule, ClassKind.BinaryRelationshipRule
                }));

                Assert.That(everything, Does.Not.Contain(ClassKind.ParameterType), "permissions are per concrete class");
                Assert.That(nothing, Is.EquivalentTo(new[] { ClassKind.ParameterizedCategoryRule, ClassKind.BinaryRelationshipRule }), "the test library has every type and category but no rule");
            });
        }

        [Test]
        public void VerifyBuildThingsToCreate()
        {
            var chain = VandVSeeder.GetChain(this.modelLibrary);
            var clone = this.modelLibrary.Clone(false);
            var created = VandVSeeder.BuildThingsToCreate(clone, VandVManifest.Check(chain), ProjectGates, chain);
            var after = VandVManifest.Check([clone, this.siteLibrary]);

            var stage = clone.ParameterType.OfType<EnumerationParameterType>().Single(x => x.ShortName == VandVConstants.Stage);
            var closure = clone.ParameterType.OfType<EnumerationParameterType>().Single(x => x.ShortName == VandVConstants.Closure);
            var verificationItem = clone.DefinedCategory.Single(x => x.ShortName == VandVConstants.VerificationItemCategory);
            var itemRule = clone.Rule.OfType<ParameterizedCategoryRule>().Single(x => x.ShortName == "VnVItemAttributesRule");
            var verifiesRule = clone.Rule.OfType<BinaryRelationshipRule>().Single(x => x.ShortName == "verifiesRule");

            Assert.Multiple(() =>
            {
                Assert.That(after.IsComplete, Is.True, "everything the manifest declares now exists");
                Assert.That(created, Is.Unique);
                Assert.That(created.OfType<EnumerationValueDefinition>().Count(), Is.EqualTo(clone.ParameterType.OfType<EnumerationParameterType>().Sum(x => x.ValueDefinition.Count)), "value definitions are written too");
                Assert.That(clone.ParameterType.All(x => x.Symbol == x.ShortName), Is.True);
                Assert.That(stage.ValueDefinition.Select(x => x.Name), Is.EqualTo(ProjectGates), "the user's stage gates, in their order");
                Assert.That(closure.ValueDefinition.Select(x => x.ShortName), Is.EqualTo(new[] { "Not_Assessed", "Closes_Out_Requirement", "Further_VV_Required" }));
                Assert.That(verificationItem.SuperCategory.Single().ShortName, Is.EqualTo(VandVConstants.VnVItemCategory));
                Assert.That(itemRule.Category.ShortName, Is.EqualTo(VandVConstants.VnVItemCategory));
                Assert.That(itemRule.ParameterType.Select(x => x.ShortName), Is.EqualTo(new[] { VandVConstants.Method, VandVConstants.Stage, VandVConstants.AcceptanceCriteria, VandVConstants.Status }));
                Assert.That(verifiesRule.TargetCategory, Is.SameAs(this.requirementCategory), "the model's own requirement category, from the site library");
                Assert.That(verifiesRule.SourceCategory.ShortName, Is.EqualTo(VandVConstants.VnVItemCategory));
                Assert.That(verifiesRule.RelationshipCategory.ShortName, Is.EqualTo(VandVConstants.VerifiesCategory));
                Assert.That(verifiesRule.InverseRelationshipName, Is.EqualTo("is verified by"));
                Assert.That(clone.DefinedCategory.Any(x => x.ShortName == "REQ"), Is.False, "nothing in the chain is created again");
            });
        }

        [Test]
        public void VerifyBuildThingsToCreateReusesTheChain()
        {
            // an older IME release seeded everything but the activity release, in the site library
            var olderRelease = new VandVTestData();
            this.siteLibrary.ParameterType.AddRange(olderRelease.Library.ParameterType.Where(x => x.ShortName != VandVConstants.Closure).ToList());
            this.siteLibrary.DefinedCategory.AddRange(olderRelease.Library.DefinedCategory.Where(x => x.ShortName is not (VandVConstants.VnVActivityCategory or VandVConstants.PerformedByCategory or VandVConstants.VnVReportCategory or "REQ")).ToList());

            var chain = VandVSeeder.GetChain(this.modelLibrary);
            var clone = this.modelLibrary.Clone(false);
            var check = VandVManifest.Check(chain);
            var created = VandVSeeder.BuildThingsToCreate(clone, check, ProjectGates, chain);
            var activityRule = clone.Rule.OfType<ParameterizedCategoryRule>().Single(x => x.ShortName == "VnVActivityAttributesRule");

            Assert.Multiple(() =>
            {
                Assert.That(clone.ParameterType.Select(x => x.ShortName), Is.EqualTo(new[] { VandVConstants.Closure }));
                Assert.That(clone.DefinedCategory.Select(x => x.ShortName), Is.EquivalentTo(new[] { VandVConstants.VnVActivityCategory, VandVConstants.PerformedByCategory, VandVConstants.VnVReportCategory }));
                Assert.That(activityRule.ParameterType, Is.EquivalentTo(this.siteLibrary.ParameterType.Where(x => x.ShortName is VandVConstants.Method or VandVConstants.Stage)), "existing types are referenced");
                Assert.That(created.OfType<EnumerationValueDefinition>().Count(), Is.EqualTo(3), "only the new closure values");
                Assert.That(VandVManifest.Check([clone, this.siteLibrary]).IsComplete, Is.True);
            });
        }

        [Test]
        public void VerifyBuildThingsToCreateWithoutRequirementCategory()
        {
            this.siteLibrary.DefinedCategory.Clear();
            var chain = VandVSeeder.GetChain(this.modelLibrary);
            var clone = this.modelLibrary.Clone(false);

            VandVSeeder.BuildThingsToCreate(clone, VandVManifest.Check(chain), ProjectGates, chain);

            Assert.Multiple(() =>
            {
                Assert.That(clone.Rule.OfType<BinaryRelationshipRule>(), Is.Empty);
                Assert.That(clone.Rule.OfType<ParameterizedCategoryRule>().Count(), Is.EqualTo(3));
                Assert.That(VandVManifest.Check([clone]).IsComplete, Is.True, "complete as far as it can be without a requirement category");
            });
        }
    }
}
