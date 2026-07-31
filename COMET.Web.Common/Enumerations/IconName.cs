// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IconName.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Enumerations
{
    /// <summary>
    /// Enumerates the glyphs of the single, consolidated COMET WEB icon system. Each member maps to one
    /// vendored Feather SVG and is rendered either through the <c>CometIcon</c> component or, for the
    /// <c>IconCssClass</c> slot of DevExpress buttons, through the CSS class returned by
    /// <see cref="COMET.Web.Common.Extensions.IconNameExtensions.GetCssClass(IconName)" />.
    /// </summary>
    public enum IconName
    {
        /// <summary>
        /// Plus glyph, used for "add" / "new" actions.
        /// </summary>
        Add,

        /// <summary>
        /// Circled plus glyph, used for prominent "add" affordances.
        /// </summary>
        AddCircle,

        /// <summary>
        /// Minus glyph, used for "remove" actions.
        /// </summary>
        Remove,

        /// <summary>
        /// Pencil glyph, used for "edit" actions.
        /// </summary>
        Edit,

        /// <summary>
        /// Waste-bin glyph, used for "delete" actions.
        /// </summary>
        Delete,

        /// <summary>
        /// Floppy-disk glyph, used for "save" actions.
        /// </summary>
        Save,

        /// <summary>
        /// Cross glyph, used for "close" / "dismiss" actions.
        /// </summary>
        Close,

        /// <summary>
        /// Check-mark glyph, used to indicate confirmation or success.
        /// </summary>
        Check,

        /// <summary>
        /// Information glyph, used for informational hints and tooltips.
        /// </summary>
        Info,

        /// <summary>
        /// Cog glyph, used for settings, options and configuration.
        /// </summary>
        Settings,

        /// <summary>
        /// Bell glyph, used for notifications and subscriptions.
        /// </summary>
        Bell,

        /// <summary>
        /// Folder glyph, used for folders and the model tree.
        /// </summary>
        Folder,

        /// <summary>
        /// File glyph, used for a generic file.
        /// </summary>
        File,

        /// <summary>
        /// Document glyph, used for text documents such as the book editor.
        /// </summary>
        FileText,

        /// <summary>
        /// Box glyph, used for element definitions and the model editor.
        /// </summary>
        Box,

        /// <summary>
        /// Layers glyph, used for iterations and stacked concepts.
        /// </summary>
        Layers,

        /// <summary>
        /// Chain-link glyph, used for links and relationships.
        /// </summary>
        Link,

        /// <summary>
        /// Branch glyph, used for expression trees and forks.
        /// </summary>
        Branch,

        /// <summary>
        /// Person glyph, used for a participant or person.
        /// </summary>
        User,

        /// <summary>
        /// Log-out glyph, used for the session log-out action.
        /// </summary>
        LogOut,

        /// <summary>
        /// Counter-clockwise rotation glyph, used for undo / restore actions.
        /// </summary>
        Undo,

        /// <summary>
        /// Circle-slash glyph, used for deprecate / disable actions.
        /// </summary>
        Ban,

        /// <summary>
        /// Target glyph, used to mark a selected or focused item.
        /// </summary>
        Target,

        /// <summary>
        /// Circular-arrow glyph, used for reload / refresh actions.
        /// </summary>
        Refresh,

        /// <summary>
        /// Repeat glyph, used for transfer / swap actions.
        /// </summary>
        Transfer,

        /// <summary>
        /// Download glyph, used for download / import actions.
        /// </summary>
        Download,

        /// <summary>
        /// Upload glyph, used for upload / export actions.
        /// </summary>
        Upload,

        /// <summary>
        /// Cloud-upload glyph, used for uploading to a store.
        /// </summary>
        UploadCloud,

        /// <summary>
        /// Database glyph, used for the data-source / session bar.
        /// </summary>
        Database,

        /// <summary>
        /// Grid glyph, used for spreadsheet / matrix views.
        /// </summary>
        Grid,

        /// <summary>
        /// Columns glyph, used for the parameter editor.
        /// </summary>
        Columns,

        /// <summary>
        /// Copy glyph, used for duplicate actions.
        /// </summary>
        Copy,

        /// <summary>
        /// Eye glyph, used to reveal hidden or deprecated items.
        /// </summary>
        Eye,

        /// <summary>
        /// Crossed-eye glyph, used to hide items.
        /// </summary>
        EyeOff,

        /// <summary>
        /// Left chevron, used for previous / collapse-left navigation.
        /// </summary>
        ChevronLeft,

        /// <summary>
        /// Right chevron, used for next / expand-right navigation.
        /// </summary>
        ChevronRight,

        /// <summary>
        /// Up chevron, used for collapse-up navigation.
        /// </summary>
        ChevronUp,

        /// <summary>
        /// Down chevron, used for expand-down navigation and drop-down carets.
        /// </summary>
        ChevronDown,

        /// <summary>
        /// Up arrow, used to move an item up in an ordered list.
        /// </summary>
        ArrowUp,

        /// <summary>
        /// Down arrow, used to move an item down in an ordered list.
        /// </summary>
        ArrowDown,

        /// <summary>
        /// Left arrow, used for backward navigation.
        /// </summary>
        ArrowLeft,

        /// <summary>
        /// Right arrow, used for forward navigation.
        /// </summary>
        ArrowRight,

        /// <summary>
        /// Activity glyph, used for the model dashboard.
        /// </summary>
        Activity,

        /// <summary>
        /// Book glyph, used for the book editor.
        /// </summary>
        Book,

        /// <summary>
        /// Layout glyph, used for the system representation.
        /// </summary>
        Layout,

        /// <summary>
        /// Package glyph, used for the viewer.
        /// </summary>
        Package,

        /// <summary>
        /// Pie-chart glyph, used for the budget / dashboard applications.
        /// </summary>
        PieChart,

        /// <summary>
        /// Server glyph, used for server-side applications.
        /// </summary>
        Server,

        /// <summary>
        /// Share glyph, used for the relationship matrix and sharing.
        /// </summary>
        Share,

        /// <summary>
        /// Smile glyph, used for empty / welcome states.
        /// </summary>
        Smile,

        /// <summary>
        /// List glyph, used for the tabs overview application.
        /// </summary>
        List,
    }
}
