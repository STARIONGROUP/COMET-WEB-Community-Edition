// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExcelExporter.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RequirementsEditor
{
    using System.Text.RegularExpressions;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using ClosedXML.Excel;

    using COMETwebapp.Model.RequirementsEditor.Export;
    using COMETwebapp.Services.Export;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    /// <summary>
    /// Exports a set of <see cref="RequirementsSpecification" />s to an Excel workbook, honouring every toggle of the
    /// <see cref="RequirementsExportConfiguration" />. Each sheet is an Excel table (so filtering and sorting are
    /// available), requirements are indented and outlined by their group depth (collapsible rows), definitions get one
    /// column per language and relationships one column per rule and direction. Built from an immutable
    /// <see cref="RequirementsExportPayload" /> so it is decoupled from the view model and unit-testable; new file
    /// formats are added as new <see cref="IExporter" /> implementations over the same payload.
    /// </summary>
    public partial class RequirementsExcelExporter : IExporter
    {
        /// <summary>
        /// Matches the characters Excel forbids in a worksheet name.
        /// </summary>
        /// <returns>The compiled, source-generated <see cref="Regex" /></returns>
        [GeneratedRegex(@"[\[\]\*/\\\?:]")]
        private static partial Regex InvalidSheetNameCharacters();

        /// <summary>
        /// Matches the characters not allowed in the downloaded file name.
        /// </summary>
        /// <returns>The compiled, source-generated <see cref="Regex" /></returns>
        [GeneratedRegex(@"[<>:""/\\|\?\*\x00-\x1F]")]
        private static partial Regex InvalidFileNameCharacters();

        /// <summary>
        /// The maximum outline level Excel supports.
        /// </summary>
        private const int MaximumOutlineLevel = 7;

        /// <summary>
        /// The fixed width, in characters, given to the wide free-text columns (definitions, constraints, relationships).
        /// </summary>
        private const double WideColumnWidth = 45;

        /// <summary>
        /// The immutable snapshot of the requirements to export.
        /// </summary>
        private readonly RequirementsExportPayload payload;

        /// <summary>
        /// The specifications actually written, after applying the deprecated filter.
        /// </summary>
        private readonly IReadOnlyList<RequirementsSpecification> specifications;

        /// <summary>
        /// Every requirement written, across all exported specifications, after the deprecated filter.
        /// </summary>
        private readonly IReadOnlyList<Requirement> allRequirements;

        /// <summary>
        /// The parameter-type value columns written for every requirement, resolved once from the configuration.
        /// </summary>
        private readonly IReadOnlyList<ParameterType> valueColumns;

        /// <summary>
        /// The definition language codes written as columns, resolved once from the configuration.
        /// </summary>
        private readonly IReadOnlyList<string> definitionLanguages;

        /// <summary>
        /// The exported relationship details of each requirement, keyed by requirement identifier and already filtered by
        /// the configured relationship categories; empty when relationships are not exported.
        /// </summary>
        private readonly Dictionary<Guid, IReadOnlyList<RequirementRelationshipDetail>> relationshipDetails = [];

        /// <summary>
        /// The ordered columns written for every requirement, resolved once from the configuration.
        /// </summary>
        private readonly List<RequirementsExportColumn> columns;

        /// <summary>
        /// The one-based index of the requirement's identity column, indented to convey group depth and carrying a
        /// group's own name on its header row.
        /// </summary>
        private readonly int identityColumnIndex;

        /// <summary>
        /// The one-based index of the "Group" column, or zero when requirements are not grouped.
        /// </summary>
        private readonly int groupColumnIndex;

        /// <summary>
        /// The worksheet names already used, so duplicate specification names get a unique sheet name.
        /// </summary>
        private readonly HashSet<string> usedSheetNames = [];

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsExcelExporter" />
        /// </summary>
        /// <param name="payload">The <see cref="RequirementsExportPayload" /> to export</param>
        public RequirementsExcelExporter(RequirementsExportPayload payload)
        {
            this.payload = payload;
            this.specifications = payload.Specifications.Where(this.IsIncluded).ToList();
            this.allRequirements = this.specifications.SelectMany(specification => specification.Requirement).Where(this.IsIncluded).ToList();
            this.valueColumns = this.ResolveValueColumns();
            this.definitionLanguages = this.ResolveDefinitionLanguages();
            this.CacheRelationshipDetails();
            this.columns = this.BuildColumns(out this.identityColumnIndex, out this.groupColumnIndex);
        }

        /// <summary>
        /// Gets the name of the exported file, from the configured name (sanitized, without a duplicated extension) or
        /// the default "Requirements".
        /// </summary>
        public string FileName => $"{this.ResolveFileName()}.xlsx";

        /// <summary>
        /// Gets the configuration driving what is written.
        /// </summary>
        private RequirementsExportConfiguration Configuration => this.payload.Configuration;

        /// <summary>
        /// Builds the Excel workbook and returns it as a readable <see cref="Stream" />.
        /// </summary>
        /// <returns>The <see cref="Stream" /> holding the workbook bytes</returns>
        public Stream Export()
        {
            using var workbook = new XLWorkbook();

            if (this.Configuration.SpecificationPerSheet)
            {
                foreach (var specification in this.specifications)
                {
                    var worksheet = workbook.Worksheets.Add(this.SafeSheetName(specification.Name));
                    this.WriteHeaderRow(worksheet);
                    var row = 2;
                    this.WriteSpecificationRows(worksheet, specification, ref row);
                    this.FinalizeWorksheet(worksheet, row - 1);
                }
            }
            else
            {
                var worksheet = workbook.Worksheets.Add(this.SafeSheetName("Requirements"));
                this.WriteHeaderRow(worksheet);
                var row = 2;

                foreach (var specification in this.specifications)
                {
                    this.WriteSpecificationRows(worksheet, specification, ref row);
                }

                this.FinalizeWorksheet(worksheet, row - 1);
            }

            if (workbook.Worksheets.Count == 0)
            {
                var worksheet = workbook.Worksheets.Add("Requirements");
                this.WriteHeaderRow(worksheet);
                this.FinalizeWorksheet(worksheet, 1);
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Writes the header row, one cell per column.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        private void WriteHeaderRow(IXLWorksheet worksheet)
        {
            for (var index = 0; index < this.columns.Count; index++)
            {
                worksheet.Cell(1, index + 1).Value = this.columns[index].Title;
            }
        }

        /// <summary>
        /// Writes the requirements of the given <paramref name="specification" /> in document order: grouped and
        /// outlined by group depth when configured, otherwise as a flat, short-name-ordered list.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        /// <param name="specification">The <see cref="RequirementsSpecification" /> to write</param>
        /// <param name="row">The row to start at; advanced past everything written</param>
        private void WriteSpecificationRows(IXLWorksheet worksheet, RequirementsSpecification specification, ref int row)
        {
            if (!this.Configuration.GroupRequirements)
            {
                foreach (var requirement in specification.Requirement.Where(this.IsIncluded).OrderBy(requirement => requirement.ShortName))
                {
                    this.WriteRequirementRow(worksheet, specification, requirement, 0, ref row);
                }

                return;
            }

            foreach (var requirement in this.OrderedRequirements(specification, null))
            {
                this.WriteRequirementRow(worksheet, specification, requirement, 0, ref row);
            }

            foreach (var group in OrderedGroups(specification.Group))
            {
                this.WriteGroupRows(worksheet, specification, group, 1, ref row);
            }
        }

        /// <summary>
        /// Writes the requirements of the given <paramref name="group" /> then recurses into its child groups, deepening
        /// the outline level.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        /// <param name="specification">The <see cref="RequirementsSpecification" /> the group belongs to</param>
        /// <param name="group">The <see cref="RequirementsGroup" /> to write</param>
        /// <param name="depth">The nesting depth of the group, one at the top level</param>
        /// <param name="row">The row to start at; advanced past everything written</param>
        private void WriteGroupRows(IXLWorksheet worksheet, RequirementsSpecification specification, RequirementsGroup group, int depth, ref int row)
        {
            this.WriteGroupHeaderRow(worksheet, group, depth, ref row);

            foreach (var requirement in this.OrderedRequirements(specification, group))
            {
                this.WriteRequirementRow(worksheet, specification, requirement, depth, ref row);
            }

            foreach (var child in OrderedGroups(group.Group))
            {
                this.WriteGroupRows(worksheet, specification, child, depth + 1, ref row);
            }
        }

        /// <summary>
        /// Writes a group's header row: its own name (not the parent path) in the identity column and its full path in
        /// the group column, bold and indented, acting as the collapsible summary of the rows beneath it.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        /// <param name="group">The <see cref="RequirementsGroup" /> to write</param>
        /// <param name="depth">The nesting depth of the group, one at the top level</param>
        /// <param name="row">The row to write at; advanced past the header</param>
        private void WriteGroupHeaderRow(IXLWorksheet worksheet, RequirementsGroup group, int depth, ref int row)
        {
            var nameCell = worksheet.Cell(row, this.identityColumnIndex);
            nameCell.Value = this.Label(group);
            nameCell.Style.Font.Bold = true;
            nameCell.Style.Alignment.Indent = Math.Min(depth - 1, 15);

            if (this.groupColumnIndex > 0)
            {
                worksheet.Cell(row, this.groupColumnIndex).Value = this.GroupPath(group);
            }

            worksheet.Row(row).OutlineLevel = Math.Min(depth - 1, MaximumOutlineLevel);
            row++;
        }

        /// <summary>
        /// Writes one requirement as a row, one cell per column, indenting its identity cell and outlining its row by the
        /// group <paramref name="depth" /> so the rows collapse and expand by group in Excel.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to write to</param>
        /// <param name="specification">The <see cref="RequirementsSpecification" /> the requirement belongs to</param>
        /// <param name="requirement">The <see cref="Requirement" /> to write</param>
        /// <param name="depth">The group depth of the requirement, zero when directly under the specification</param>
        /// <param name="row">The row to write at; advanced past the requirement</param>
        private void WriteRequirementRow(IXLWorksheet worksheet, RequirementsSpecification specification, Requirement requirement, int depth, ref int row)
        {
            for (var index = 0; index < this.columns.Count; index++)
            {
                worksheet.Cell(row, index + 1).Value = this.columns[index].Value(requirement, specification);
            }

            if (this.Configuration.GroupRequirements && depth > 0)
            {
                worksheet.Cell(row, this.identityColumnIndex).Style.Alignment.Indent = Math.Min(depth, 15);
                worksheet.Row(row).OutlineLevel = Math.Min(depth, MaximumOutlineLevel);
            }

            row++;
        }

        /// <summary>
        /// Turns the written range into an Excel table (so filtering and sorting are available), widens and wraps the
        /// free-text columns, sizes the rest to their content and freezes the header.
        /// </summary>
        /// <param name="worksheet">The <see cref="IXLWorksheet" /> to finalize</param>
        /// <param name="lastRow">The last written row (the header row when there is no data)</param>
        private void FinalizeWorksheet(IXLWorksheet worksheet, int lastRow)
        {
            if (lastRow >= 2)
            {
                worksheet.Range(1, 1, lastRow, this.columns.Count).CreateTable();
            }
            else
            {
                worksheet.Row(1).Style.Font.Bold = true;
            }

            worksheet.Outline.SummaryVLocation = XLOutlineSummaryVLocation.Top;
            worksheet.Columns().Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
            worksheet.Columns().AdjustToContents();

            for (var index = 0; index < this.columns.Count; index++)
            {
                if (!this.columns[index].Wide)
                {
                    continue;
                }

                worksheet.Column(index + 1).Width = WideColumnWidth;
                worksheet.Column(index + 1).Style.Alignment.WrapText = true;
            }

            worksheet.SheetView.FreezeRows(1);
        }

        /// <summary>
        /// Builds the ordered columns from the configuration and reports the one-based indexes of the identity and group
        /// columns.
        /// </summary>
        /// <param name="identityColumn">The one-based index of the requirement's identity column</param>
        /// <param name="groupColumn">The one-based index of the group column, or zero when requirements are not grouped</param>
        /// <returns>The ordered columns</returns>
        private List<RequirementsExportColumn> BuildColumns(out int identityColumn, out int groupColumn)
        {
            var result = new List<RequirementsExportColumn>();

            this.AddSpecificationColumn(result);
            identityColumn = result.Count + 1;
            this.AddIdentityColumns(result);
            this.AddDefinitionColumns(result);
            this.AddOwnerColumn(result);
            this.AddCategoriesColumn(result);
            groupColumn = this.AddGroupColumn(result);
            this.AddDeprecatedColumn(result);
            this.AddValueColumns(result);
            result.AddRange(this.BuildRelationshipColumns());
            this.AddConstraintsColumn(result);

            EnsureUniqueTitles(result);
            return result;
        }

        /// <summary>
        /// Adds the "Specification" column when specifications share a single worksheet, so a row can still be traced
        /// back to its specification.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddSpecificationColumn(List<RequirementsExportColumn> result)
        {
            if (!this.Configuration.SpecificationPerSheet)
            {
                result.Add(new RequirementsExportColumn("Specification", (_, specification) => this.Label(specification), false));
            }
        }

        /// <summary>
        /// Adds the requirement's identity column(s): "Short Name" and "Name" for
        /// <see cref="RequirementsExportNamingMode.Both" />, otherwise the single configured one.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddIdentityColumns(List<RequirementsExportColumn> result)
        {
            if (this.Configuration.NamingMode == RequirementsExportNamingMode.Both)
            {
                result.Add(new RequirementsExportColumn("Short Name", (requirement, _) => requirement.ShortName, false));
                result.Add(new RequirementsExportColumn("Name", (requirement, _) => requirement.Name, false));
                return;
            }

            var byName = this.Configuration.NamingMode == RequirementsExportNamingMode.Name;
            result.Add(new RequirementsExportColumn(byName ? "Name" : "Short Name", (requirement, _) => byName ? requirement.Name : requirement.ShortName, false));
        }

        /// <summary>
        /// Adds one "Definition (&lt;language&gt;)" column per resolved <see cref="definitionLanguages" /> entry, when
        /// definitions are included.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddDefinitionColumns(List<RequirementsExportColumn> result)
        {
            if (this.Configuration.IncludeDefinitions)
            {
                result.AddRange(this.definitionLanguages.Select(language => new RequirementsExportColumn($"Definition ({language})", (requirement, _) => Definition(requirement, language), true)));
            }
        }

        /// <summary>
        /// Adds the "Owner" column, when configured.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddOwnerColumn(List<RequirementsExportColumn> result)
        {
            if (this.Configuration.IncludeOwner)
            {
                result.Add(new RequirementsExportColumn("Owner", (requirement, _) => requirement.Owner == null ? string.Empty : this.Label(requirement.Owner), false));
            }
        }

        /// <summary>
        /// Adds the "Categories" column, when configured.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddCategoriesColumn(List<RequirementsExportColumn> result)
        {
            if (this.Configuration.IncludeCategories)
            {
                result.Add(new RequirementsExportColumn("Categories", (requirement, _) => string.Join(", ", requirement.Category.Select(this.Label)), false));
            }
        }

        /// <summary>
        /// Adds the "Group" column, when requirements are grouped.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        /// <returns>The one-based index of the added column, or zero when requirements are not grouped</returns>
        private int AddGroupColumn(List<RequirementsExportColumn> result)
        {
            if (!this.Configuration.GroupRequirements)
            {
                return 0;
            }

            result.Add(new RequirementsExportColumn("Group", (requirement, _) => requirement.Group == null ? string.Empty : this.Label(requirement.Group), false));
            return result.Count;
        }

        /// <summary>
        /// Adds the "Deprecated" column, when configured.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddDeprecatedColumn(List<RequirementsExportColumn> result)
        {
            if (this.Configuration.IncludeDeprecated)
            {
                result.Add(new RequirementsExportColumn("Deprecated", (requirement, _) => requirement.IsDeprecated ? "Yes" : string.Empty, false));
            }
        }

        /// <summary>
        /// Adds one column per resolved <see cref="valueColumns" /> parameter type.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddValueColumns(List<RequirementsExportColumn> result)
        {
            result.AddRange(this.valueColumns.Select(parameterType => new RequirementsExportColumn(this.Label(parameterType), (requirement, _) => SimpleParameterValue(requirement, parameterType), false)));
        }

        /// <summary>
        /// Adds the "Parametric Constraints" column, when configured.
        /// </summary>
        /// <param name="result">The columns to add to</param>
        private void AddConstraintsColumn(List<RequirementsExportColumn> result)
        {
            if (this.Configuration.IncludeParametricConstraints)
            {
                result.Add(new RequirementsExportColumn("Parametric Constraints", (requirement, _) => string.Join("\n", requirement.ParametricConstraint.Select(this.payload.GetConstraintText)), true));
            }
        }

        /// <summary>
        /// Builds the relationship columns: a forward and an inverse column for every directional rule, one column for
        /// every non-directional rule, and a single generic "Relationships" column when any relationship matches no rule.
        /// </summary>
        /// <returns>The relationship columns</returns>
        private List<RequirementsExportColumn> BuildRelationshipColumns()
        {
            if (this.Configuration.Relationships == RequirementsExportSelectionMode.None)
            {
                return [];
            }

            var details = this.relationshipDetails.Values.SelectMany(x => x).ToList();
            var result = new List<RequirementsExportColumn>();

            var rules = details
                .Where(detail => detail.Rule != null)
                .Select(detail => detail.Rule)
                .DistinctBy(rule => rule.Iid)
                .OrderBy(rule => rule.Name)
                .ToList();

            foreach (var rule in rules)
            {
                var ruleIid = rule.Iid;

                if (rule.IsDirectional)
                {
                    result.Add(new RequirementsExportColumn(rule.ForwardName, (requirement, _) => this.RelationshipCell(requirement, ruleIid, RelationshipDirection.Outgoing), true));
                    result.Add(new RequirementsExportColumn(rule.InverseName, (requirement, _) => this.RelationshipCell(requirement, ruleIid, RelationshipDirection.Incoming), true));
                }
                else
                {
                    result.Add(new RequirementsExportColumn(rule.Name, (requirement, _) => this.RelationshipCell(requirement, ruleIid, RelationshipDirection.Bidirectional), true));
                }
            }

            if (details.Any(detail => detail.Rule == null))
            {
                result.Add(new RequirementsExportColumn("Relationships", (requirement, _) => this.RelationshipCell(requirement, null, null), true));
            }

            return result;
        }

        /// <summary>
        /// Gets the related-thing labels of the given <paramref name="requirement" /> for one relationship column: the
        /// details matching the given <paramref name="ruleIid" /> (or the rule-less details when it is null) and the
        /// given <paramref name="direction" /> (any direction when it is null).
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="ruleIid">The rule identifier, or null for the rule-less generic column</param>
        /// <param name="direction">The direction to match, or null to match any</param>
        /// <returns>The joined related-thing labels, or an empty string when there are none</returns>
        private string RelationshipCell(Requirement requirement, Guid? ruleIid, RelationshipDirection? direction)
        {
            // every requirement reaching this cell was already cached by CacheRelationshipDetails, so the lookup cannot miss
            var matching = this.relationshipDetails[requirement.Iid]
                .Where(detail => detail.Rule?.Iid == ruleIid && (direction == null || detail.Direction == direction))
                .SelectMany(detail => detail.RelatedThings)
                .Select(this.ThingLabel);

            // a trailing comma on every line but the last makes multiple related things read as distinct entries
            return string.Join(",\n", matching);
        }

        /// <summary>
        /// Caches, per requirement, the relationship details filtered by the configured relationship categories; does
        /// nothing when relationships are not exported.
        /// </summary>
        private void CacheRelationshipDetails()
        {
            if (this.Configuration.Relationships == RequirementsExportSelectionMode.None)
            {
                return;
            }

            var selectedCategoryIids = this.Configuration.SelectedRelationshipCategories.Select(x => x.Iid).ToHashSet();

            foreach (var requirement in this.allRequirements)
            {
                var details = this.payload.GetRelationshipDetails(requirement);

                if (this.Configuration.Relationships == RequirementsExportSelectionMode.Selection)
                {
                    details = details.Where(detail => detail.Categories.Any(category => selectedCategoryIids.Contains(category.Iid))).ToList();
                }

                this.relationshipDetails[requirement.Iid] = details;
            }
        }

        /// <summary>
        /// Resolves the parameter-type value columns from the configuration: none, the selected types, or every type
        /// used by the exported requirements.
        /// </summary>
        /// <returns>The parameter types to write as columns, in short-name order</returns>
        private List<ParameterType> ResolveValueColumns()
        {
            switch (this.Configuration.SimpleParameterValues)
            {
                case RequirementsExportSelectionMode.None:
                    return [];

                case RequirementsExportSelectionMode.Selection:
                    return this.Configuration.SelectedParameterTypes.OrderBy(x => x.ShortName).ToList();

                default:
                    return this.allRequirements
                        .SelectMany(requirement => requirement.ParameterValue)
                        .Select(value => value.ParameterType)
                        .DistinctBy(parameterType => parameterType.Iid)
                        .OrderBy(parameterType => parameterType.ShortName)
                        .ToList();
            }
        }

        /// <summary>
        /// Resolves the definition language codes written as columns: the single configured language, or every language
        /// used by the exported requirements' definitions.
        /// </summary>
        /// <returns>The language codes, in alphabetical order</returns>
        private List<string> ResolveDefinitionLanguages()
        {
            if (!this.Configuration.IncludeDefinitions)
            {
                return [];
            }

            if (!string.IsNullOrWhiteSpace(this.Configuration.DefinitionLanguageCode))
            {
                return [this.Configuration.DefinitionLanguageCode];
            }

            return this.allRequirements
                .SelectMany(requirement => requirement.Definition)
                .Select(definition => definition.LanguageCode)
                .Where(languageCode => !string.IsNullOrWhiteSpace(languageCode))
                .Distinct()
                .OrderBy(languageCode => languageCode)
                .ToList();
        }

        /// <summary>
        /// Gets the definition content of the given <paramref name="requirement" /> for the given
        /// <paramref name="language" />, its definitions joined by new lines.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="language">The language code</param>
        /// <returns>The joined definition content, or an empty string when there is none</returns>
        private static string Definition(Requirement requirement, string language)
        {
            return string.Join("\n", requirement.Definition
                .Where(definition => string.Equals(definition.LanguageCode, language, StringComparison.OrdinalIgnoreCase))
                .Select(definition => definition.Content));
        }

        /// <summary>
        /// Gets the simple parameter value of the given <paramref name="requirement" /> for the given
        /// <paramref name="parameterType" />, its components joined by commas.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="parameterType">The <see cref="ParameterType" /></param>
        /// <returns>The joined value, or an empty string when the requirement has no value for that type</returns>
        private static string SimpleParameterValue(Requirement requirement, ParameterType parameterType)
        {
            var value = requirement.ParameterValue.FirstOrDefault(x => x.ParameterType?.Iid == parameterType.Iid);
            return value == null ? string.Empty : string.Join(", ", value.Value);
        }

        /// <summary>
        /// Gets the visible requirements of the given <paramref name="specification" /> located directly under the given
        /// <paramref name="group" /> (or the specification when it is null), ordered by short name.
        /// </summary>
        /// <param name="specification">The <see cref="RequirementsSpecification" /></param>
        /// <param name="group">The <see cref="RequirementsGroup" />, or null for the requirements directly under the specification</param>
        /// <returns>The requirements to write</returns>
        private IEnumerable<Requirement> OrderedRequirements(RequirementsSpecification specification, RequirementsGroup group)
        {
            return specification.Requirement
                .Where(requirement => ReferenceEquals(requirement.Group, group) && this.IsIncluded(requirement))
                .OrderBy(requirement => requirement.ShortName);
        }

        /// <summary>
        /// Orders the given <paramref name="groups" /> by short name.
        /// </summary>
        /// <param name="groups">The <see cref="RequirementsGroup" />s</param>
        /// <returns>The ordered groups</returns>
        private static IEnumerable<RequirementsGroup> OrderedGroups(IEnumerable<RequirementsGroup> groups)
        {
            return groups.OrderBy(group => group.ShortName);
        }

        /// <summary>
        /// Determines whether the given deprecatable <paramref name="thing" /> is included, respecting the
        /// <see cref="RequirementsExportConfiguration.IncludeDeprecated" /> toggle.
        /// </summary>
        /// <param name="thing">The <see cref="IDeprecatableThing" /></param>
        /// <returns>true when the thing should be written</returns>
        private bool IsIncluded(IDeprecatableThing thing)
        {
            return this.Configuration.IncludeDeprecated || !thing.IsDeprecated;
        }

        /// <summary>
        /// Gets the label of the given <paramref name="thing" /> according to the configured
        /// <see cref="RequirementsExportNamingMode" />.
        /// </summary>
        /// <param name="thing">The <see cref="DefinedThing" /></param>
        /// <returns>The label</returns>
        private string Label(DefinedThing thing)
        {
            return this.Configuration.NamingMode switch
            {
                RequirementsExportNamingMode.ShortName => thing.ShortName,
                RequirementsExportNamingMode.Name => thing.Name,
                _ => $"{thing.Name} ({thing.ShortName})"
            };
        }

        /// <summary>
        /// Gets the full path of the given <paramref name="group" />, its ancestor groups' labels joined by " / " from
        /// the top-level group down to the group itself.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <returns>The path</returns>
        private string GroupPath(RequirementsGroup group)
        {
            var labels = new List<string>();

            for (var ancestor = group; ancestor != null; ancestor = ancestor.Container as RequirementsGroup)
            {
                labels.Add(this.Label(ancestor));
            }

            labels.Reverse();
            return string.Join(" / ", labels);
        }

        /// <summary>
        /// Gets the label of the given related <paramref name="thing" />: its <see cref="Label(DefinedThing)" /> when it
        /// is a <see cref="DefinedThing" />, otherwise its user-friendly name.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /></param>
        /// <returns>The label</returns>
        private string ThingLabel(Thing thing)
        {
            return thing is DefinedThing definedThing ? this.Label(definedThing) : thing.UserFriendlyName;
        }

        /// <summary>
        /// Makes the titles of the given <paramref name="result" /> columns unique, appending a numeric suffix to
        /// duplicates, because an Excel table forbids two columns with the same header.
        /// </summary>
        /// <param name="result">The columns to disambiguate in place</param>
        private static void EnsureUniqueTitles(List<RequirementsExportColumn> result)
        {
            var used = new HashSet<string>();

            foreach (var column in result)
            {
                var candidate = string.IsNullOrEmpty(column.Title) ? "Column" : column.Title;
                var title = candidate;
                var suffix = 2;

                while (!used.Add(title))
                {
                    title = $"{candidate} ({suffix++})";
                }

                column.Title = title;
            }
        }

        /// <summary>
        /// Resolves the exported file name (without extension) from the configuration: the user's name with any
        /// forbidden character and trailing ".xlsx" removed, or the default "Requirements" when it is empty.
        /// </summary>
        /// <returns>The file name without extension</returns>
        private string ResolveFileName()
        {
            var configured = (this.Configuration.FileName ?? string.Empty).Trim();

            if (configured.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                configured = configured[..^5];
            }

            var sanitized = InvalidFileNameCharacters().Replace(configured, string.Empty).Trim();
            return string.IsNullOrEmpty(sanitized) ? "Requirements" : sanitized;
        }

        /// <summary>
        /// Builds a valid, unique Excel worksheet name from the given <paramref name="name" />: forbidden characters are
        /// replaced, it is truncated to 31 characters, and a numeric suffix disambiguates duplicates.
        /// </summary>
        /// <param name="name">The desired sheet name</param>
        /// <returns>A safe, unique worksheet name</returns>
        private string SafeSheetName(string name)
        {
            var sanitized = InvalidSheetNameCharacters().Replace(name ?? string.Empty, " ").Trim();

            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = "Specification";
            }

            if (sanitized.Length > 31)
            {
                sanitized = sanitized[..31];
            }

            var candidate = sanitized;
            var suffix = 1;

            while (!this.usedSheetNames.Add(candidate))
            {
                var tag = $" ({suffix++})";
                candidate = sanitized.Length + tag.Length > 31 ? sanitized[..(31 - tag.Length)] + tag : sanitized + tag;
            }

            return candidate;
        }
    }
}
