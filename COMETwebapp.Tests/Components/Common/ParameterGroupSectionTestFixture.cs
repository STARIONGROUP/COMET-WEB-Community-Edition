// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterGroupSectionTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Common
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using NUnit.Framework;

    /// <summary>
    ///     Unit tests for the <see cref="ParameterGroupSection" /> component.
    /// </summary>
    [TestFixture]
    public class ParameterGroupSectionTestFixture
    {
        /// <summary>
        ///     The bunit <see cref="BunitContext" /> used to render the component under test.
        /// </summary>
        private BunitContext context;

        /// <summary>
        ///     Initializes the bunit context before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
        }

        /// <summary>
        ///     Disposes of the bunit context after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        ///     Builds a <see cref="Parameter" /> owned by the supplied <see cref="DomainOfExpertise" /> and
        ///     contained by a fresh <see cref="ElementDefinition" />.
        /// </summary>
        private static Parameter BuildParameter(DomainOfExpertise owner, string typeName, string typeShortName)
        {
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = typeName, ShortName = typeShortName };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = owner,
                ParameterType = parameterType
            };

            parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new CDP4Common.Types.ValueArray<string>(["1"]),
                Computed = new CDP4Common.Types.ValueArray<string>(["1"]),
                Reference = new CDP4Common.Types.ValueArray<string>(["-"]),
                Formula = new CDP4Common.Types.ValueArray<string>(["-"]),
                Published = new CDP4Common.Types.ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            var containingDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                ShortName = "CONT",
                Owner = owner
            };

            containingDefinition.Parameter.Add(parameter);

            return parameter;
        }

        [Test]
        public void VerifyNamedGroupRendersHeaderAndCard()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };

            var parameter = BuildParameter(owner, "Temperature", "T");
            parameter.Group = group;

            var row = new ElementDefinitionDetailsRowViewModel(parameter, owner);

            var rendered = this.context.Render<ParameterGroupSection>(parameters => parameters
                .Add(p => p.Group, group)
                .Add(p => p.AllRows, new List<ElementDefinitionDetailsRowViewModel> { row })
                .Add(p => p.AllGroups, new List<ParameterGroup> { group })
                .Add(p => p.SearchTerm, string.Empty)
                .Add(p => p.Level, 0));

            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("group-card"), "Group card wrapper must render.");
                Assert.That(markup, Does.Contain("group-card-header"), "Group card header must render.");
                Assert.That(markup, Does.Contain("Thermal"), "Header must contain the group name.");
                Assert.That(markup, Does.Contain("parameter-card"), "At least one parameter card must render.");
                Assert.That(markup, Does.Contain("Temperature"), "Parameter type name must appear inside the card.");
            });
        }

        [Test]
        public void VerifyUngroupedSectionRendersWithNullGroup()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };

            var parameter = BuildParameter(owner, "Mass", "m");
            // parameter.Group is null — belongs to Ungrouped

            var row = new ElementDefinitionDetailsRowViewModel(parameter, owner);

            var rendered = this.context.Render<ParameterGroupSection>(parameters => parameters
                .Add(p => p.Group, (ParameterGroup)null)
                .Add(p => p.AllRows, new List<ElementDefinitionDetailsRowViewModel> { row })
                .Add(p => p.AllGroups, new List<ParameterGroup>())
                .Add(p => p.SearchTerm, string.Empty)
                .Add(p => p.Level, 0));

            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("group-card"), "Group card wrapper must render for the Ungrouped section.");
                Assert.That(markup, Does.Contain("ungrouped"), "Ungrouped CSS class must be applied.");
                Assert.That(markup, Does.Contain("Ungrouped"), "Section header must say 'Ungrouped'.");
                Assert.That(markup, Does.Contain("parameter-card"), "Ungrouped parameter card must render.");
            });
        }

        [Test]
        public void VerifySearchFilterHidesNonMatchingGroup()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };

            var parameter = BuildParameter(owner, "Temperature", "T");
            parameter.Group = group;

            var row = new ElementDefinitionDetailsRowViewModel(parameter, owner);

            // Search term that does not match "Temperature"
            var rendered = this.context.Render<ParameterGroupSection>(parameters => parameters
                .Add(p => p.Group, group)
                .Add(p => p.AllRows, new List<ElementDefinitionDetailsRowViewModel> { row })
                .Add(p => p.AllGroups, new List<ParameterGroup> { group })
                .Add(p => p.SearchTerm, "XYZ_NOMATCH")
                .Add(p => p.Level, 0));

            // When no rows match and no children have content, the section renders nothing.
            Assert.That(rendered.Markup, Does.Not.Contain("group-card"));
        }

        [Test]
        public void VerifySearchFilterKeepsMatchingGroup()
        {
            var owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };

            var parameter = BuildParameter(owner, "Temperature", "T");
            parameter.Group = group;

            var row = new ElementDefinitionDetailsRowViewModel(parameter, owner);

            var rendered = this.context.Render<ParameterGroupSection>(parameters => parameters
                .Add(p => p.Group, group)
                .Add(p => p.AllRows, new List<ElementDefinitionDetailsRowViewModel> { row })
                .Add(p => p.AllGroups, new List<ParameterGroup> { group })
                .Add(p => p.SearchTerm, "temp")
                .Add(p => p.Level, 0));

            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("group-card"), "Matching group must still render.");
                Assert.That(markup, Does.Contain("Temperature"), "Matching parameter must still render.");
            });
        }
    }
}
