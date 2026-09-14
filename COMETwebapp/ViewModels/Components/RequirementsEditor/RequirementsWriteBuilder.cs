// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsWriteBuilder.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Builds the cloned <see cref="Thing" />s a create/update write of a <see cref="RequirementsSpecification" />,
    /// <see cref="RequirementsGroup" /> or <see cref="Requirement" /> needs, mirroring the container-cloning rules the
    /// server enforces on each of them.
    /// </summary>
    public static class RequirementsWriteBuilder
    {
        /// <summary>
        /// Clones the container the given <paramref name="thing" /> is written into, appending the clones to
        /// <paramref name="thingsToWrite" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> being created or updated.</param>
        /// <param name="thingsToWrite">The things to create or update, extended with the container clone.</param>
        /// <param name="iteration">The <see cref="Iteration" /> the write happens against.</param>
        /// <param name="isCreating">True when a fresh instance is being created, false when an existing clone is being updated.</param>
        /// <param name="creationParent">The <see cref="RequirementsContainer" /> the new group or requirement is placed under; only used while creating.</param>
        /// <returns>The top container of the write, or null when the <paramref name="thing" /> is not supported.</returns>
        public static Thing PrepareTopContainer(Thing thing, List<Thing> thingsToWrite, Iteration iteration, bool isCreating, RequirementsContainer creationParent)
        {
            return thing switch
            {
                RequirementsSpecification specification => PrepareSpecificationWrite(specification, thingsToWrite, iteration, isCreating),
                RequirementsGroup group => PrepareGroupWrite(group, thingsToWrite, isCreating, creationParent),
                Requirement requirement => PrepareRequirementWrite(requirement, thingsToWrite, isCreating, creationParent),
                _ => null
            };
        }

        /// <summary>
        /// Clones the <see cref="Iteration" /> a <see cref="RequirementsSpecification" /> is written into, adding the
        /// specification to it when it is being created.
        /// </summary>
        /// <param name="specification">The <see cref="RequirementsSpecification" /> being created or updated.</param>
        /// <param name="thingsToWrite">The things to create or update, extended with the iteration clone.</param>
        /// <param name="iteration">The <see cref="Iteration" /> the write happens against.</param>
        /// <param name="isCreating">True when the specification is being created, false when it is being updated.</param>
        /// <returns>The iteration clone.</returns>
        public static Thing PrepareSpecificationWrite(RequirementsSpecification specification, List<Thing> thingsToWrite, Iteration iteration, bool isCreating)
        {
            var iterationClone = iteration.Clone(false);

            if (isCreating)
            {
                specification.Container = iteration;
                iterationClone.RequirementsSpecification.Add(specification);
            }

            thingsToWrite.Add(iterationClone);
            return iterationClone;
        }

        /// <summary>
        /// Clones the <see cref="RequirementsContainer" /> a <see cref="RequirementsGroup" /> is written into, adding the
        /// group to it when it is being created.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /> being created or updated.</param>
        /// <param name="thingsToWrite">The things to create or update, extended with the container clone.</param>
        /// <param name="isCreating">True when the group is being created, false when it is being updated.</param>
        /// <param name="creationParent">The <see cref="RequirementsContainer" /> the new group is placed under; only used while creating.</param>
        /// <returns>The container clone.</returns>
        public static Thing PrepareGroupWrite(RequirementsGroup group, List<Thing> thingsToWrite, bool isCreating, RequirementsContainer creationParent)
        {
            Thing containerClone;

            if (isCreating)
            {
                var parentClone = creationParent.Clone(false);
                group.Container = creationParent;
                parentClone.Group.Add(group);
                containerClone = parentClone;
            }
            else
            {
                containerClone = group.Container.Clone(false);
            }

            thingsToWrite.Add(containerClone);
            return containerClone;
        }

        /// <summary>
        /// Clones the <see cref="RequirementsSpecification" /> a <see cref="Requirement" /> is written into, adding the
        /// requirement to it when it is being created.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> being created or updated.</param>
        /// <param name="thingsToWrite">The things to create or update, extended with the specification clone.</param>
        /// <param name="isCreating">True when the requirement is being created, false when it is being updated.</param>
        /// <param name="creationParent">The <see cref="RequirementsContainer" /> the new requirement is filed under; only used while creating.</param>
        /// <returns>The specification clone.</returns>
        public static Thing PrepareRequirementWrite(Requirement requirement, List<Thing> thingsToWrite, bool isCreating, RequirementsContainer creationParent)
        {
            var specification = isCreating
                ? creationParent as RequirementsSpecification ?? creationParent.GetContainerOfType<RequirementsSpecification>()
                : requirement.GetContainerOfType<RequirementsSpecification>();

            var specificationClone = specification.Clone(false);

            if (isCreating)
            {
                requirement.Container = specification;
                specificationClone.Requirement.Add(requirement);
            }

            thingsToWrite.Add(specificationClone);
            return specificationClone;
        }

        /// <summary>
        /// Adds the simple parameter values, parametric constraints and their expressions of a <see cref="Requirement" />
        /// to <paramref name="thingsToWrite" />, and collects the expressions the rebuilt constraints no longer reference.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> being created or updated.</param>
        /// <param name="thingsToWrite">The things to create or update, extended with the requirement's contained things.</param>
        /// <returns>The expressions to delete; empty when the <paramref name="thing" /> is not a <see cref="Requirement" />.</returns>
        public static List<Thing> CollectRequirementThings(Thing thing, List<Thing> thingsToWrite)
        {
            var expressionsToDelete = new List<Thing>();

            if (thing is not Requirement requirement)
            {
                return expressionsToDelete;
            }

            thingsToWrite.AddRange(requirement.ParameterValue);

            foreach (ParametricConstraint constraint in requirement.ParametricConstraint)
            {
                thingsToWrite.AddRange(constraint.Expression);
                thingsToWrite.Add(constraint);
                expressionsToDelete.AddRange(GetDiscardedExpressions(constraint));
            }

            return expressionsToDelete;
        }

        /// <summary>
        /// Gets the clones of the <see cref="BooleanExpression" />s the given <paramref name="constraint" /> held before it was
        /// edited and that its rebuilt expression tree no longer contains, so that they are deleted rather than left orphaned
        /// inside the constraint. An expression is discarded either because the user removed it, or because it had to be
        /// re-created under a new identity to keep the write acceptable to the server.
        /// </summary>
        /// <param name="constraint">The edited <see cref="ParametricConstraint" /> clone.</param>
        /// <returns>The <see cref="BooleanExpression" /> clones to delete.</returns>
        public static IEnumerable<Thing> GetDiscardedExpressions(ParametricConstraint constraint)
        {
            if (constraint.Original is not ParametricConstraint original)
            {
                return [];
            }

            return original.Expression
                .Where(expression => constraint.Expression.All(x => x.Iid != expression.Iid))
                .Select(expression =>
                {
                    var clone = expression.Clone(false);
                    clone.Container = constraint;
                    return (Thing)clone;
                });
        }
    }
}
