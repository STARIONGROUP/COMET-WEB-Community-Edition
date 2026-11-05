// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVEndToEndData.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using CDP4ServicesDal;

    using COMETwebapp.Utilities.VandV;

    /// <summary>
    /// Writes the V&amp;V data the end-to-end tests read, directly through the SDK as the web app cannot create V&amp;V items
    /// yet and the end-to-end database holds no requirements. It uses a specification of its own and retires only what it
    /// created, but an existing V&amp;V plan keeps the deprecated items, so run it against the end-to-end stack only.
    /// </summary>
    internal sealed class VandVEndToEndData
    {
        /// <summary>
        /// The <see cref="ISession" /> the data is written with.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The open <see cref="Iteration" /> of the model under test.
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The iids of what <see cref="RetireAsync" /> retires: the relationships, the specification and the items.
        /// </summary>
        private readonly HashSet<Guid> created = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVEndToEndData" /> class.
        /// </summary>
        /// <param name="session">The open <see cref="ISession" />.</param>
        /// <param name="iteration">The open <see cref="Iteration" />.</param>
        private VandVEndToEndData(ISession session, Iteration iteration)
        {
            this.session = session;
            this.iteration = iteration;

            var suffix = Guid.NewGuid().ToString("N")[..8];
            this.SpecificationShortName = $"E2EVV_{suffix}";
            this.UncoveredRequirementShortName = $"E2EREQ_UNCOVERED_{suffix}";
            this.PassedItemShortName = $"VNV_PASSED_{suffix}";
            this.PlannedItemShortName = $"VNV_PLANNED_{suffix}";
        }

        /// <summary>
        /// Gets the short name of the specification holding the requirements.
        /// </summary>
        public string SpecificationShortName { get; }

        /// <summary>
        /// Gets the short name of the requirement without an item.
        /// </summary>
        public string UncoveredRequirementShortName { get; }

        /// <summary>
        /// Gets the short name of the item that passed, at the CDR stage gate.
        /// </summary>
        public string PassedItemShortName { get; }

        /// <summary>
        /// Gets the short name of the item that is planned, at the PDR stage gate.
        /// </summary>
        public string PlannedItemShortName { get; }

        /// <summary>
        /// Opens a session of its own on the first model, iteration and domain the given user can select, the ones
        /// <c>TabsPageModel.OpenApplicationTabAsync</c> opens, and writes the data there. The reference data must be set up.
        /// </summary>
        /// <param name="serverUrl">The COMET Web Services URL.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The written data.</returns>
        public static async Task<VandVEndToEndData> CreateAsync(string serverUrl, string userName, string password)
        {
            var session = new Session(new CdpServicesDal((IAuthenticationRefreshService)null), new Credentials(userName, password, new Uri(serverUrl)), new CDPMessageBus());

            try
            {
                await session.Open();

                var modelSetup = session.RetrieveSiteDirectory().Model
                    .Where(x => x.Participant.Exists(participant => participant.Person.Iid == session.ActivePerson.Iid))
                    .OrderBy(x => x.Name)
                    .First();

                var iterationSetup = modelSetup.IterationSetup.Where(x => !x.IsDeleted).OrderBy(x => x.IterationNumber).First();
                var domain = modelSetup.Participant.First(x => x.Person.Iid == session.ActivePerson.Iid).Domain.OrderBy(x => x.Name).First();

                var model = new EngineeringModel(modelSetup.EngineeringModelIid, session.Assembler.Cache, session.Credentials.Uri) { EngineeringModelSetup = modelSetup };
                var iteration = new Iteration(iterationSetup.IterationIid, session.Assembler.Cache, session.Credentials.Uri) { IterationSetup = iterationSetup };
                model.Iteration.Add(iteration);
                await session.Read(iteration, domain);

                var data = new VandVEndToEndData(session, session.OpenIterations.Keys.Single(x => x.Iid == iteration.Iid));
                await data.WriteAsync(domain, VandVSeeder.GetChain(modelSetup.RequiredRdl.Single()));
                return data;
            }
            catch
            {
                await session.Close();
                throw;
            }
        }

        /// <summary>
        /// Deletes the relationships this instance created, deprecates its specification and items, and closes its session. The
        /// SDK refuses to delete a deprecatable thing, which is also why the app only offers to deprecate them. A plan it had
        /// to create stays, as a deprecated one would still be found by its short name the next time.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task RetireAsync()
        {
            try
            {
                await this.session.Refresh();

                var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.iteration));

                foreach (var relationship in this.iteration.Relationship.Where(x => this.created.Contains(x.Iid)))
                {
                    transaction.Delete(relationship.Clone(false), relationship.Container);
                }

                foreach (var specification in this.iteration.RequirementsSpecification.Where(x => this.created.Contains(x.Iid)))
                {
                    var clone = specification.Clone(false);
                    clone.IsDeprecated = true;
                    transaction.CreateOrUpdate(clone);
                }

                foreach (var requirement in this.iteration.RequirementsSpecification.SelectMany(x => x.Requirement).Where(x => this.created.Contains(x.Iid)))
                {
                    var clone = requirement.Clone(false);
                    clone.IsDeprecated = true;
                    transaction.CreateOrUpdate(clone);
                }

                await this.session.Write(transaction.FinalizeTransaction());
            }
            finally
            {
                await this.session.Close();
            }
        }

        /// <summary>
        /// Writes the specification, its requirements, the items and their coverage in one transaction.
        /// </summary>
        /// <param name="owner">The owning <see cref="DomainOfExpertise" />.</param>
        /// <param name="libraries">The chain of reference data libraries of the model.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task WriteAsync(DomainOfExpertise owner, IReadOnlyList<ReferenceDataLibrary> libraries)
        {
            var parameterTypes = libraries.SelectMany(x => x.ParameterType).ToList();
            var categories = libraries.SelectMany(x => x.DefinedCategory).ToList();
            var itemCategory = categories.First(x => x.ShortName == VandVConstants.VnVItemCategory);
            var verifies = categories.First(x => x.ShortName == VandVConstants.VerifiesCategory);

            var iterationClone = this.iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iterationClone));

            var specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = this.SpecificationShortName, Name = this.SpecificationShortName, Owner = owner };
            iterationClone.RequirementsSpecification.Add(specification);

            var existingPlan = this.iteration.RequirementsSpecification.Find(x => x.ShortName == VandVConstants.VandVSpecificationShortName);
            var plan = existingPlan?.Clone(false) ?? new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = VandVConstants.VandVSpecificationShortName, Name = "V&V Plan", Owner = owner };

            if (existingPlan == null)
            {
                iterationClone.RequirementsSpecification.Add(plan);
            }

            List<Thing> things = [iterationClone, specification, plan];

            Requirement AddRequirement(RequirementsSpecification container, string shortName)
            {
                var requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = shortName, Name = shortName, Owner = owner };
                container.Requirement.Add(requirement);
                things.Add(requirement);
                return requirement;
            }

            void SetValue(Requirement requirement, string parameterTypeShortName, string value)
            {
                var parameterValue = new SimpleParameterValue
                {
                    Iid = Guid.NewGuid(),
                    ParameterType = parameterTypes.First(x => x.ShortName == parameterTypeShortName),
                    Value = new ValueArray<string>([value])
                };

                requirement.ParameterValue.Add(parameterValue);
                things.Add(parameterValue);
            }

            Requirement AddItem(string shortName, Requirement covered, string stage, string status)
            {
                var item = AddRequirement(plan, shortName);
                item.Category.Add(itemCategory);
                SetValue(item, VandVConstants.Stage, stage);
                SetValue(item, VandVConstants.Status, status);

                var coverage = new BinaryRelationship { Iid = Guid.NewGuid(), Owner = owner, Source = item, Target = covered };
                coverage.Category.Add(verifies);
                iterationClone.Relationship.Add(coverage);
                things.Add(coverage);
                this.created.Add(coverage.Iid);

                return item;
            }

            var passedRequirement = AddRequirement(specification, this.PassedItemShortName.Replace("VNV_", "E2EREQ_"));
            var plannedRequirement = AddRequirement(specification, this.PlannedItemShortName.Replace("VNV_", "E2EREQ_"));
            AddRequirement(specification, this.UncoveredRequirementShortName);

            var passedItem = AddItem(this.PassedItemShortName, passedRequirement, "CDR", VandVConstants.StatusPassed);
            var plannedItem = AddItem(this.PlannedItemShortName, plannedRequirement, "PDR", VandVConstants.StatusPlanned);
            this.created.Add(specification.Iid);

            this.created.Add(passedItem.Iid);
            this.created.Add(plannedItem.Iid);

            foreach (var thing in things)
            {
                transaction.CreateOrUpdate(thing);
            }

            await this.session.Write(transaction.FinalizeTransaction());
        }
    }
}
