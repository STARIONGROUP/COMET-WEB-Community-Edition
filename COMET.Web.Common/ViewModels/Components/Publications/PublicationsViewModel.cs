// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationsViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Publications
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the Publications component
    /// </summary>
    public class PublicationsViewModel : DisposableObject, IPublicationsViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="Iteration"/> that's being used
        /// </summary>
        public Iteration CurrentIteration { get; private set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterOrOverrideBase"/> that can be published
        /// </summary>
        private List<ParameterOrOverrideBase> ParametersToBePublished { get; set; } = new();

        /// <summary>
        /// Gets or sets the rows used in the Publications component
        /// </summary>
        public SourceList<PublicationRowViewModel> Rows { get; set; } = new();

        /// <summary>
        /// Gets the <see cref="ISessionService"/>
        /// </summary>
        private ISessionService SessionService { get; }

        /// <summary>
        /// Backing field for the <see cref="CanPublish"/> property
        /// </summary>
        private bool canPublish;

        /// <summary>
        /// Gets or sets if the publication is possible
        /// </summary>
        public bool CanPublish
        {
            get => this.canPublish;
            set => this.RaiseAndSetIfChanged(ref this.canPublish, value);
        }

        /// <summary>
        /// Creates a new instance of type <see cref="PublicationsViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        public PublicationsViewModel(ISessionService sessionService)
        {
            this.SessionService = sessionService;
        }

        /// <summary>
        /// Updates the properties of this ViewModel
        /// </summary>
        /// <param name="iteration">the iteration to use</param>
        public void UpdateProperties(Iteration iteration)
        {
            this.CurrentIteration = iteration;
            this.ParametersToBePublished = iteration.QueryParameterAndOverrideBases().Where(x => x.ToBePublished).ToList();
            this.CanPublish = this.ParametersToBePublished.Any();

            this.Rows.Clear();
            this.Rows.AddRange(this.CreateRows(this.ParametersToBePublished));
        }

        /// <summary>
        /// Creates the rows used for the Publications component
        /// </summary>
        /// <param name="parameters">the parameters used to create the rows</param>
        /// <returns>A collection of <see cref="PublicationRowViewModel"/></returns>
        private IEnumerable<PublicationRowViewModel> CreateRows(IEnumerable<ParameterOrOverrideBase> parameters)
        {
            return parameters.SelectMany(param =>
                param.ValueSets.Select(valueSet => new PublicationRowViewModel(param, valueSet))).ToList();
        }

        /// <summary>
        /// Execute the publication.
        /// </summary>
        public async Task ExecutePublish()
        {
            if (this.CanPublish)
            {
                return;
            }

            var publication = new Publication(Guid.NewGuid(), null, null);
            var iteration = this.CurrentIteration.Clone(false);

            iteration.Publication.Add(publication);

            publication.Container = iteration;

            publication.PublishedParameter = this.ParametersToBePublished;

            await this.SessionService.UpdateThings(iteration, new List<Thing> { publication });

            //var transactionContext = TransactionContextResolver.ResolveContext(this.CurrentIteration);
            //var containerTransaction = new ThingTransaction(transactionContext, iteration);
            //containerTransaction.CreateOrUpdate(publication);

            //try
            //{
            //    var operationContainer = containerTransaction.FinalizeTransaction();
            //    await this.Session.Write(operationContainer);

            //    // Unselecect the domain rows
            //    foreach (var domain in this.Domains)
            //    {
            //        domain.ToBePublished = false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(
            //        string.Format("Publication failed: {0}", ex.Message),
            //        "Publication Failed",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}
            //finally
            //{
            //    this.IsBusy = false;
            //}
        }
    }
}
