// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BatchParameterEditorViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using ReactiveUI;

    /// <summary>
    /// ViewModel used to apply batch operations for a parameter
    /// </summary>
    public class BatchParameterEditorViewModel : DisposableObject, IBatchParameterEditorViewModel
    {
        /// <summary>
        /// Creates a new instance of type <see cref="BatchParameterEditorViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public BatchParameterEditorViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.SessionService = sessionService;

            var valueSet = new ParameterValueSet()
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["-"])
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterTypeSelector.SelectedParameterType).Subscribe(selectedParameterType =>
            {
                this.ParameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(selectedParameterType, valueSet, false, messageBus, 0);
            }));
        }

        /// <summary>
        /// Gets the current <see cref="Iteration"/>
        /// </summary>
        public Iteration CurrentIteration { get; private set; }

        /// <summary>
        /// Gets the <see cref="ISessionService"/>
        /// </summary>
        public ISessionService SessionService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ParameterTypeEditorSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelector { get; private set; } = new ParameterTypeSelectorViewModel();

        /// <summary>
        /// Sets the current iteration
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        public void SetCurrentIteration(Iteration iteration)
        {
            this.CurrentIteration = iteration;
            this.ParameterTypeSelector.CurrentIteration = iteration;
        }
    }
}
