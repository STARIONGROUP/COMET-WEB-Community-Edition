// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationTreeViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    using NUnit.Framework;

    /// <summary>
    /// Unit tests for <see cref="SystemRepresentationTreeViewModel" />.
    /// </summary>
    [TestFixture]
    public class SystemRepresentationTreeViewModelTestFixture
    {
        /// <summary>
        /// Verifies that the three display-option toggles default to <c>true</c> on a freshly constructed
        /// <see cref="SystemRepresentationTreeViewModel" />.
        /// </summary>
        [Test]
        public void VerifyDisplayOptionDefaults()
        {
            using var viewModel = new SystemRepresentationTreeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.ShowName, Is.True);
                Assert.That(viewModel.ShowOwner, Is.True);
                Assert.That(viewModel.ShowCategories, Is.True);
            });
        }

        /// <summary>
        /// Verifies that <see cref="SystemRepresentationTreeViewModel.CanDrop" /> correctly admits or rejects
        /// drop operations across a representative set of scenarios.
        /// </summary>
        [Test]
        public void VerifyCanDrop()
        {
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };

            var rootDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Root", ShortName = "ROOT", Owner = domain };
            var childDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Child", ShortName = "CHILD", Owner = domain };
            var childUsage = new ElementUsage { Iid = Guid.NewGuid(), Name = "ChildUsage", ShortName = "CU", Owner = domain, ElementDefinition = childDefinition };
            rootDefinition.ContainedElement.Add(childUsage);

            var rootNode = new SystemNodeViewModel(rootDefinition);
            var childNode = new SystemNodeViewModel(childUsage);
            rootNode.AddChild(childNode);

            // Null arguments → false
            Assert.That(SystemRepresentationTreeViewModel.CanDrop(null, rootNode), Is.False,
                "A null 'from' node must be rejected.");

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(rootNode, null), Is.False,
                "A null 'to' node must be rejected.");

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(null, null), Is.False,
                "Both null must be rejected.");

            // Same node → false
            Assert.That(SystemRepresentationTreeViewModel.CanDrop(rootNode, rootNode), Is.False,
                "Dropping a node onto itself must be rejected.");

            // Dropping root onto its own descendant → false (would create a cycle)
            Assert.That(SystemRepresentationTreeViewModel.CanDrop(rootNode, childNode), Is.False,
                "Dropping a node onto one of its own descendants must be rejected to avoid cycles.");

            // Two unrelated element nodes → true
            var unrelatedDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Unrelated", ShortName = "UNR", Owner = domain };
            var unrelatedNode = new SystemNodeViewModel(unrelatedDefinition);

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(unrelatedNode, rootNode), Is.True,
                "Dropping an unrelated element node onto the root must be permitted.");

            // Child node dropped onto the root (inverted — valid, no cycle) → true
            Assert.That(SystemRepresentationTreeViewModel.CanDrop(childNode, rootNode), Is.True,
                "Dropping a child node back onto the root is a valid operation.");
            
            var rootUsage = new ElementUsage { Iid = Guid.NewGuid(), Name = "RootUsage", ShortName = "RU", Owner = domain, ElementDefinition = rootDefinition };
            var rootUsageNode = new SystemNodeViewModel(rootUsage);
            var freshRootNode = new SystemNodeViewModel(rootDefinition);

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(freshRootNode, rootUsageNode), Is.False,
                "Dropping an element definition onto a separate usage of the same definition must be rejected to avoid a self-containment cycle.");

            // A node whose Thing is not an ElementBase → false, on either side.
            var nonElementNode = new SystemNodeViewModel(rootDefinition);
            nonElementNode.SetThing(new Option { Iid = Guid.NewGuid(), Name = "Option", ShortName = "OPT" });

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(nonElementNode, rootNode), Is.False,
                "A dragged node whose Thing is not an ElementBase must be rejected.");

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(rootNode, nonElementNode), Is.False,
                "A target node whose Thing is not an ElementBase must be rejected.");

            // A target usage with no ElementDefinition resolves to a null definition → false.
            var danglingUsage = new ElementUsage { Iid = Guid.NewGuid(), Name = "Dangling", ShortName = "DU", Owner = domain };
            var danglingNode = new SystemNodeViewModel(danglingUsage);

            Assert.That(SystemRepresentationTreeViewModel.CanDrop(unrelatedNode, danglingNode), Is.False,
                "Dropping onto a usage node with no ElementDefinition must be rejected.");
        }

        /// <summary>
        /// Verifies that <see cref="SystemRepresentationTreeViewModel.CreateTree" /> excludes
        /// <see cref="ElementUsage" /> nodes whose <see cref="ElementUsage.ExcludeOption" /> contains the
        /// selected <see cref="Option" />, and includes usages that are not excluded from that option.
        /// </summary>
        [Test]
        public void VerifyCreateTreeFiltersUsagesByOption()
        {
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            var option = new Option { Iid = Guid.NewGuid(), Name = "Option A", ShortName = "OA" };
            var otherOption = new Option { Iid = Guid.NewGuid(), Name = "Option B", ShortName = "OB" };

            var rootDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Root", ShortName = "ROOT", Owner = domain };
            var childDefA = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Alpha", ShortName = "A", Owner = domain };
            var childDefB = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Beta", ShortName = "B", Owner = domain };

            // usageA: excluded from the selected option — must NOT appear in the tree
            var usageA = new ElementUsage { Iid = Guid.NewGuid(), Name = "UsageA", ShortName = "UA", Owner = domain, ElementDefinition = childDefA };
            usageA.ExcludeOption.Add(option);

            // usageB: not excluded from the selected option — must appear in the tree
            var usageB = new ElementUsage { Iid = Guid.NewGuid(), Name = "UsageB", ShortName = "UB", Owner = domain, ElementDefinition = childDefB };

            rootDefinition.ContainedElement.Add(usageA);
            rootDefinition.ContainedElement.Add(usageB);

            using var viewModel = new SystemRepresentationTreeViewModel();

            var root = viewModel.CreateTree(
                [rootDefinition, childDefA, childDefB],
                option,
                []);

            var allNodes = root.GetFlatListOfDescendants(true);

            Assert.Multiple(() =>
            {
                Assert.That(allNodes.Any(n => n.Thing?.Iid == usageA.Iid), Is.False,
                    "UsageA is excluded from the selected option and must not appear in the tree.");
                Assert.That(allNodes.Any(n => n.Thing?.Iid == usageB.Iid), Is.True,
                    "UsageB is not excluded from the selected option and must appear in the tree.");
            });
        }

        /// <summary>
        /// Verifies that <see cref="SystemRepresentationTreeViewModel.OnSearchFilterChange" /> sets
        /// <see cref="COMETwebapp.ViewModels.Components.Shared.BaseNodeViewModel{T}.IsDrawn" /> to <c>false</c>
        /// on non-matching nodes and <c>true</c> on matching nodes, and auto-expands ancestor branches.
        /// </summary>
        [Test]
        public void VerifyOnSearchFilterChangeFiltersNodesAndExpandsAncestors()
        {
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            var option = new Option { Iid = Guid.NewGuid(), Name = "Default", ShortName = "DEF" };

            var rootDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Satellite", ShortName = "SAT", Owner = domain };
            var childDefMatch = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Antenna Unit", ShortName = "ANT", Owner = domain };
            var childDefNoMatch = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Battery", ShortName = "BAT", Owner = domain };

            var usageMatch = new ElementUsage { Iid = Guid.NewGuid(), Name = "Antenna Unit", ShortName = "ANT", Owner = domain, ElementDefinition = childDefMatch };
            var usageNoMatch = new ElementUsage { Iid = Guid.NewGuid(), Name = "Battery", ShortName = "BAT", Owner = domain, ElementDefinition = childDefNoMatch };

            rootDefinition.ContainedElement.Add(usageMatch);
            rootDefinition.ContainedElement.Add(usageNoMatch);

            using var viewModel = new SystemRepresentationTreeViewModel();

            viewModel.CreateTree([rootDefinition, childDefMatch, childDefNoMatch], option, []);

            // Apply a search that matches the usage name "Antenna Unit" but not "Battery"
            viewModel.SearchText = "Antenna";

            var rootNode = viewModel.RootViewModel;
            var allNodes = rootNode.GetFlatListOfDescendants(true);

            var matchNode = allNodes.FirstOrDefault(n => n.Thing?.Iid == usageMatch.Iid);
            var noMatchNode = allNodes.FirstOrDefault(n => n.Thing?.Iid == usageNoMatch.Iid);

            Assert.Multiple(() =>
            {
                Assert.That(matchNode, Is.Not.Null, "The matching usage node must be in the tree.");
                Assert.That(matchNode.IsDrawn, Is.True,
                    "The matching node must have IsDrawn = true after the search filter.");
                Assert.That(noMatchNode, Is.Not.Null, "The non-matching usage node must be in the tree.");
                Assert.That(noMatchNode.IsDrawn, Is.False,
                    "The non-matching node must have IsDrawn = false after the search filter.");
                Assert.That(rootNode.IsExpanded, Is.True,
                    "The root ancestor must be auto-expanded when a descendant matches the search term.");
            });

            // Clear the search — all nodes must become visible again
            viewModel.SearchText = string.Empty;
            var allNodesAfterClear = rootNode.GetFlatListOfDescendants(true);

            Assert.That(allNodesAfterClear.All(n => n.IsDrawn), Is.True,
                "Clearing the search text must restore all nodes to IsDrawn = true.");
        }

        /// <summary>
        /// Verifies that the product-tree search also matches on a node's owning
        /// <see cref="DomainOfExpertise" /> and its <see cref="Category" />s, not only Name/ShortName.
        /// </summary>
        [Test]
        public void VerifyOnSearchFilterChangeMatchesOwnerAndCategories()
        {
            var systemDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), Name = "System", ShortName = "SYS" };
            var aeroDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), Name = "Aerodynamics", ShortName = "AER" };
            var option = new Option { Iid = Guid.NewGuid(), Name = "Default", ShortName = "DEF" };
            var propulsion = new Category { Iid = Guid.NewGuid(), Name = "Propulsion", ShortName = "PROP" };

            var rootDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Satellite", ShortName = "SAT", Owner = systemDomain };
            var childDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Thruster", ShortName = "THR", Owner = aeroDomain };
            childDefinition.Category.Add(propulsion);

            var usage = new ElementUsage { Iid = Guid.NewGuid(), Name = "Thruster", ShortName = "THR", Owner = aeroDomain, ElementDefinition = childDefinition };
            rootDefinition.ContainedElement.Add(usage);

            using var viewModel = new SystemRepresentationTreeViewModel();
            viewModel.CreateTree([rootDefinition, childDefinition], option, []);

            var usageNode = viewModel.RootViewModel.GetFlatListOfDescendants(true).FirstOrDefault(n => n.Thing?.Iid == usage.Iid);
            Assert.That(usageNode, Is.Not.Null);

            viewModel.SearchText = "AER";
            Assert.That(usageNode.IsDrawn, Is.True, "A search on the owning domain short name must match the node.");

            viewModel.SearchText = "Propulsion";
            Assert.That(usageNode.IsDrawn, Is.True, "A search on the (inherited) category name must match the node.");

            viewModel.SearchText = "nomatchterm";
            Assert.That(usageNode.IsDrawn, Is.False, "A term matching neither name, owner nor category must hide the node.");
        }

        /// <summary>
        /// Verifies that updating or removing an element that is not currently in the tree (e.g. one filtered
        /// out by the selected Option) is tolerated silently — it must never throw, since it runs inside the
        /// session End-Update notification where an exception fails the whole write.
        /// </summary>
        [Test]
        public void VerifyUpdateAndRemoveToleratesElementsNotInTree()
        {
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            var option = new Option { Iid = Guid.NewGuid(), Name = "Default", ShortName = "DEF" };
            var rootDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Root", ShortName = "ROOT", Owner = domain };
            var childDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Child", ShortName = "CH", Owner = domain };
            var usage = new ElementUsage { Iid = Guid.NewGuid(), Name = "Usage", ShortName = "U", Owner = domain, ElementDefinition = childDefinition };
            rootDefinition.ContainedElement.Add(usage);

            using var viewModel = new SystemRepresentationTreeViewModel();
            viewModel.CreateTree([rootDefinition, childDefinition], option, []);

            var absentUsage = new ElementUsage { Iid = Guid.NewGuid(), Name = "Absent", ShortName = "AB", Owner = domain, ElementDefinition = childDefinition };

            Assert.Multiple(() =>
            {
                Assert.That(() => viewModel.UpdateElementsFromTree([absentUsage]), Throws.Nothing);
                Assert.That(() => viewModel.RemoveElementsFromTree([absentUsage]), Throws.Nothing);
            });
        }
    }
}
