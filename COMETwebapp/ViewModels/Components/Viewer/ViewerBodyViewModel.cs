// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerBodyViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Viewer.ViewerBody" />
    /// </summary>
    public class ViewerBodyViewModel : SingleIterationApplicationBaseViewModel, IViewerBodyViewModel
    {
        /// <summary>
        /// Creates a new instance of type <see cref="ViewerBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="selectionMediator"> the <see cref="ISelectionMediator" /></param>
        /// <param name="babylonInterop">the <see cref="IBabylonInterop" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public ViewerBodyViewModel(ISessionService sessionService, ISelectionMediator selectionMediator, IBabylonInterop babylonInterop, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.SelectionMediator = selectionMediator;
            this.ProductTreeViewModel = new ViewerProductTreeViewModel(selectionMediator);
            this.CanvasViewModel = new CanvasViewModel(babylonInterop, selectionMediator);
            this.PropertiesViewModel = new PropertiesComponentViewModel(babylonInterop, sessionService, selectionMediator, this.MessageBus);
            this.MultipleFiniteStateSelector = new MultipleActualFiniteStateSelectorViewModel();

            this.Disposables.Add(this.WhenAnyValue(x => x.MultipleFiniteStateSelector.SelectedFiniteStates,
                    x => x.OptionSelector.SelectedOption)
                .Subscribe(_ => this.InitializeElementsAndCreateTree()));
        }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator" />
        /// </summary>
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; private set; } = new OptionSelectorViewModel(false);

        /// <summary>
        /// Gets or sets the <see cref="IMultipleActualFiniteStateSelectorViewModel" />
        /// </summary>
        public IMultipleActualFiniteStateSelectorViewModel MultipleFiniteStateSelector { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewerProductTreeViewModel" />
        /// </summary>
        public ViewerProductTreeViewModel ProductTreeViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ICanvasViewModel" />
        /// </summary>
        public ICanvasViewModel CanvasViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IPropertiesComponentViewModel" />
        /// </summary>
        public IPropertiesComponentViewModel PropertiesViewModel { get; private set; }

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; }

        /// <summary>
        /// Initializes this <see cref="IViewerBodyViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task InitializeViewModel()
        {
            this.IsLoading = true;
            await Task.Delay(1);

            this.InitializeElementsAndCreateTree();
            this.IsLoading = false;
        }

        /// <summary>
        /// Create the <see cref="ElementBase" /> based on the current <see cref="Iteration" />
        /// </summary>
        public IEnumerable<ElementBase> InitializeElements()
        {
            return this.CurrentThing?.QueryElementsBase().ToList() ?? new List<ElementBase>();
        }

        /// <summary>
        /// Initializes the elements and creates the tree based on that elements
        /// </summary>
        public void InitializeElementsAndCreateTree()
        {
            this.Elements = this.InitializeElements().ToList();

            if (this.OptionSelector.SelectedOption != null && this.MultipleFiniteStateSelector.SelectedFiniteStates.Count()
                == this.MultipleFiniteStateSelector.ActualFiniteStateSelectorViewModels.Count())
            {
                this.ProductTreeViewModel.CreateTree(this.Elements, this.OptionSelector.SelectedOption, this.MultipleFiniteStateSelector.SelectedFiniteStates);
            }
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            return this.OnThingChanged();
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.OptionSelector.CurrentIteration = this.CurrentThing;
            this.MultipleFiniteStateSelector.CurrentIteration = this.CurrentThing;

            await this.InitializeViewModel();
        }
    }
}
