// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParametricConstraintWriteOrderTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4DalCommon.Protocol.Operations;

    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using NUnit.Framework;

    [TestFixture]
    public class ParametricConstraintWriteOrderTestFixture
    {
        private readonly Uri uri = new("https://example.com");
        private CDPMessageBus messageBus;
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Requirement requirement;
        private SimpleQuantityKind parameterType;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.cache = this.assembler.Cache;

            var model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri);

            model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.specification);
            this.specification.Requirement.Add(this.requirement);

            this.cache.TryAdd(new CacheKey(model.Iid, null), new Lazy<Thing>(() => model));
            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.cache.TryAdd(new CacheKey(this.specification.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.specification));
            this.cache.TryAdd(new CacheKey(this.requirement.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.requirement));
            this.cache.TryAdd(new CacheKey(this.parameterType.Iid, null), new Lazy<Thing>(() => this.parameterType));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyExpressionsAreCreatedBeforeTheConstraint()
        {
            var requirementClone = (Requirement)this.requirement.Clone(true);

            var viewModel = new EditParametricConstraintViewModel { AvailableParameterTypes = [this.parameterType] };
            viewModel.AddNode(null, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.EQ, Value = "5" });
            viewModel.AddNode(null, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.GT, Value = "3" });

            var constraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            viewModel.BuildInto(constraint);
            requirementClone.ParametricConstraint.Add(constraint);

            var specificationClone = (RequirementsSpecification)this.specification.Clone(false);

            // Mirrors the order the Body view model assembles: for a new constraint the expressions come before it.
            var thingsToWrite = new List<Thing> { requirementClone, specificationClone };
            AddConstraintThings(thingsToWrite, requirementClone);

            var context = TransactionContextResolver.ResolveContext(specificationClone);
            var transaction = new ThingTransaction(context);

            foreach (var thing in thingsToWrite)
            {
                transaction.CreateOrUpdate(thing);
            }

            var operations = transaction.FinalizeTransaction().Operations.ToList();
            var createKinds = operations.Where(x => x.OperationKind is OperationKind.Create).Select(x => x.ModifiedThing.ClassKind).ToList();

            var firstRelationalIndex = createKinds.IndexOf(ClassKind.RelationalExpression);
            var constraintIndex = createKinds.IndexOf(ClassKind.ParametricConstraint);
            var constraintDto = (CDP4Common.DTO.ParametricConstraint)operations.Single(x => x.ModifiedThing.ClassKind == ClassKind.ParametricConstraint).ModifiedThing;

            Assert.Multiple(() =>
            {
                Assert.That(firstRelationalIndex, Is.GreaterThanOrEqualTo(0), "The relational expression creates must be in the operation.");
                Assert.That(constraintIndex, Is.GreaterThanOrEqualTo(0));

                // The server validates "a ParametricConstraint must contain at least 1 RelationalExpression" as it processes
                // the constraint create, so the relational expression creates must come first in the operation.
                Assert.That(firstRelationalIndex, Is.LessThan(constraintIndex), "Relational expressions must be created before their constraint.");
                Assert.That(constraintDto.Expression, Is.Not.Empty, "The constraint DTO must reference its expressions.");
            });
        }

        [Test]
        public void VerifyAddingANotNeverUpdatesAPersistedExpressionWithANewTerm()
        {
            // Existing constraint: A AND B (T root), all in the cache as if from the server.
            var relationalA = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType, RelationalOperator = RelationalOperatorKind.GT, Value = new ValueArray<string>(new[] { "1" }) };
            var relationalB = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType, RelationalOperator = RelationalOperatorKind.LT, Value = new ValueArray<string>(new[] { "9" }) };
            var andExpression = new AndExpression(Guid.NewGuid(), this.cache, this.uri);
            andExpression.Term.Add(relationalA);
            andExpression.Term.Add(relationalB);
            var existingConstraint = new ParametricConstraint(Guid.NewGuid(), this.cache, this.uri);
            existingConstraint.Expression.Add(relationalA);
            existingConstraint.Expression.Add(relationalB);
            existingConstraint.Expression.Add(andExpression);
            existingConstraint.TopExpression = andExpression;
            this.requirement.ParametricConstraint.Add(existingConstraint);
            this.cache.TryAdd(new CacheKey(existingConstraint.Iid, this.iteration.Iid), new Lazy<Thing>(() => existingConstraint));
            this.cache.TryAdd(new CacheKey(relationalA.Iid, this.iteration.Iid), new Lazy<Thing>(() => relationalA));
            this.cache.TryAdd(new CacheKey(relationalB.Iid, this.iteration.Iid), new Lazy<Thing>(() => relationalB));
            this.cache.TryAdd(new CacheKey(andExpression.Iid, this.iteration.Iid), new Lazy<Thing>(() => andExpression));

            var requirementClone = (Requirement)this.requirement.Clone(true);
            var constraintClone = requirementClone.ParametricConstraint.Single();

            var viewModel = new EditParametricConstraintViewModel { AvailableParameterTypes = [this.parameterType] };
            viewModel.LoadFrom(constraintClone);

            // Toggle NOT on the first leaf (A).
            var root = (CompositeExpressionRow)viewModel.RootExpression;
            viewModel.ToggleNot(root.Terms[0]);

            viewModel.BuildInto(constraintClone);

            var specificationClone = (RequirementsSpecification)this.specification.Clone(false);

            var thingsToWrite = new List<Thing> { requirementClone, specificationClone };
            AddConstraintThings(thingsToWrite, requirementClone);

            var context = TransactionContextResolver.ResolveContext(specificationClone);
            var transaction = new ThingTransaction(context);

            foreach (var thing in thingsToWrite)
            {
                transaction.CreateOrUpdate(thing);
            }

            var operations = transaction.FinalizeTransaction().Operations.ToList();

            // The expressions that already existed on the server when the transaction was built. The server validates a Term
            // update of a persisted expression against exactly this list (the constraint's Expression as stored in the
            // database), so any term outside it is rejected with "a term from outside the parametric constraint".
            var persistedExpressionIds = new HashSet<Guid> { relationalA.Iid, relationalB.Iid, andExpression.Iid };

            var newNot = operations.Single(x => x.ModifiedThing.ClassKind == ClassKind.NotExpression);
            var updatedComposites = operations
                .Where(x => x.OperationKind == OperationKind.Update && x.ModifiedThing is CDP4Common.DTO.AndExpression)
                .Select(x => (CDP4Common.DTO.AndExpression)x.ModifiedThing)
                .ToList();

            Assert.Multiple(() =>
            {
                // Adding the NOT necessarily creates a new NotExpression...
                Assert.That(newNot.OperationKind, Is.EqualTo(OperationKind.Create));

                // ...so the composite that has to reference it cannot be updated in place: every expression that is still
                // updated (rather than re-created) may only reference terms the server already has persisted.
                foreach (var composite in updatedComposites)
                {
                    Assert.That(composite.Term, Is.SubsetOf(persistedExpressionIds), "An updated expression may not reference a term created by the same transaction.");
                }

                // The AndExpression that gains the NOT as a term is therefore re-created, and its persisted version discarded.
                Assert.That(operations.Any(x => x.OperationKind == OperationKind.Create && x.ModifiedThing.ClassKind == ClassKind.AndExpression), Is.True, "The composite referencing the new term is re-created.");
                Assert.That(constraintClone.Expression.Select(x => x.Iid), Does.Not.Contain(andExpression.Iid), "The persisted composite is discarded from the constraint.");
            });
        }

        /// <summary>
        /// Mirrors <c>RequirementsEditorBodyViewModel.OnEditValidSubmitAsync</c>: a constraint's expressions are written
        /// before the constraint itself.
        /// </summary>
        private static void AddConstraintThings(List<Thing> thingsToWrite, Requirement requirement)
        {
            foreach (ParametricConstraint constraint in requirement.ParametricConstraint)
            {
                thingsToWrite.AddRange(constraint.Expression);
                thingsToWrite.Add(constraint);
            }
        }

        [Test]
        public void VerifyEditingAConstraintWithRelationalAboveComposite()
        {
            // An existing constraint (one relational) already on the requirement and in the cache.
            var oldRelational = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType, RelationalOperator = RelationalOperatorKind.EQ, Value = new ValueArray<string>(new[] { "1" }) };
            var existingConstraint = new ParametricConstraint(Guid.NewGuid(), this.cache, this.uri);
            existingConstraint.Expression.Add(oldRelational);
            existingConstraint.TopExpression = oldRelational;
            this.requirement.ParametricConstraint.Add(existingConstraint);
            this.cache.TryAdd(new CacheKey(existingConstraint.Iid, this.iteration.Iid), new Lazy<Thing>(() => existingConstraint));
            this.cache.TryAdd(new CacheKey(oldRelational.Iid, this.iteration.Iid), new Lazy<Thing>(() => oldRelational));

            // Edit it: deep-clone the requirement, rebuild the tree as  relational AND (a XOR b).
            var requirementClone = (Requirement)this.requirement.Clone(true);
            var constraintClone = requirementClone.ParametricConstraint.Single();

            var viewModel = new EditParametricConstraintViewModel { AvailableParameterTypes = [this.parameterType] };
            viewModel.LoadFrom(constraintClone);
            viewModel.AddNode(null, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.GT, Value = "2" });
            var group = viewModel.AddGroup((CompositeExpressionRow)viewModel.RootExpression);
            viewModel.AddNode(group, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.LT, Value = "3" });
            viewModel.AddNode(group, new RelationalExpressionRow { ParameterType = this.parameterType, Operator = RelationalOperatorKind.GE, Value = "4" });

            viewModel.BuildInto(constraintClone);

            var specificationClone = (RequirementsSpecification)this.specification.Clone(false);

            var thingsToWrite = new List<Thing> { requirementClone, specificationClone };
            AddConstraintThings(thingsToWrite, requirementClone);

            var context = TransactionContextResolver.ResolveContext(specificationClone);
            var transaction = new ThingTransaction(context);

            foreach (var thing in thingsToWrite)
            {
                transaction.CreateOrUpdate(thing);
            }

            var operations = transaction.FinalizeTransaction().Operations.ToList();

            var constraintOp = operations.Single(x => x.ModifiedThing.ClassKind == ClassKind.ParametricConstraint);
            var constraintDto = (CDP4Common.DTO.ParametricConstraint)constraintOp.ModifiedThing;

            Assert.Multiple(() =>
            {
                // The edit is a real in-place update of the existing constraint...
                Assert.That(constraintOp.OperationKind, Is.EqualTo(OperationKind.Update));
                Assert.That(constraintOp.ModifiedThing.Iid, Is.EqualTo(existingConstraint.Iid));

                // ...and the constraint still references the pre-existing relational expression (its identity is preserved),
                // so the server sees a RelationalExpression that already exists in the database when it validates the update.
                Assert.That(constraintDto.Expression, Does.Contain(oldRelational.Iid), "The existing relational expression's identity is preserved on edit.");

                // The pre-existing relational is UPDATED in place (the reused clone), not recreated with a duplicate id
                // (which the server rejects with "Container update of item ... is missing from the operation").
                var oldRelationalOp = operations.Single(x => x.ModifiedThing.Iid == oldRelational.Iid);
                Assert.That(oldRelationalOp.OperationKind, Is.EqualTo(OperationKind.Update));
            });
        }
    }
}
