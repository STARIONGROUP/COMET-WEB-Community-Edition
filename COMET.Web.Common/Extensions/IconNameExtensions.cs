// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IconNameExtensions.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Extensions
{
    using System.Text;

    using COMET.Web.Common.Enumerations;

    /// <summary>
    /// Extension methods for <see cref="IconName" />.
    /// </summary>
    public static class IconNameExtensions
    {
        /// <summary>
        /// The CSS class of every <see cref="IconName" />, computed once at type initialisation so the per-render
        /// <see cref="GetCssClass(IconName)" /> is a plain dictionary lookup rather than repeated string work.
        /// </summary>
        private static readonly Dictionary<IconName, string> CssClasses = Enum.GetValues<IconName>()
            .ToDictionary(icon => icon, icon => $"comet-icon comet-icon-{icon.ToCssSuffix()}");

        /// <param name="icon">The <see cref="IconName" /> to render.</param>
        extension(IconName icon)
        {
            /// <summary>
            /// Gets the CSS class string that renders the given <see cref="IconName" /> as a mask-based glyph, for
            /// use both by the <c>CometIcon</c> component and directly in the <c>IconCssClass</c> slot of a DevExpress
            /// button or menu item (where a component cannot be passed).
            /// </summary>
            /// <returns>
            /// The two-token class string, for example <c>"comet-icon comet-icon-add"</c>, whose per-glyph mask rule
            /// is emitted at runtime by the <c>CometIconStyles</c> component from the vendored Feather icon library.
            /// </returns>
            public string GetCssClass()
            {
                return CssClasses.TryGetValue(icon, out var cssClass)
                    ? cssClass
                    : $"comet-icon comet-icon-{icon.ToCssSuffix()}";
            }

            /// <summary>
            /// Gets the kebab-case suffix for the given <see cref="IconName" />, for example <c>AddCircle</c> becomes
            /// <c>add-circle</c>. This suffix names the per-glyph CSS class.
            /// </summary>
            /// <returns>The kebab-case suffix.</returns>
            public string ToCssSuffix()
            {
                var name = icon.ToString();
                var builder = new StringBuilder(name.Length + 4);

                for (var index = 0; index < name.Length; index++)
                {
                    var character = name[index];

                    if (index > 0 && char.IsUpper(character))
                    {
                        builder.Append('-');
                    }

                    builder.Append(char.ToLowerInvariant(character));
                }

                return builder.ToString();
            }

            /// <summary>
            /// Maps an <see cref="IconName" /> to the corresponding Feather icon name (the identifier the vendored
            /// Feather icon library uses to look up the SVG), so the semantic vocabulary stays decoupled from the
            /// underlying library's naming.
            /// </summary>
            /// <returns>The Feather icon name, for example <see cref="IconName.Add" /> maps to <c>plus</c>.</returns>
            public string ToFeatherName()
            {
                return icon switch
                {
                    IconName.Add => "plus",
                    IconName.AddCircle => "plus-circle",
                    IconName.Remove => "minus",
                    IconName.Edit => "edit-2",
                    IconName.Delete => "trash-2",
                    IconName.Save => "save",
                    IconName.Close => "x",
                    IconName.Check => "check",
                    IconName.Info => "info",
                    IconName.Settings => "settings",
                    IconName.Bell => "bell",
                    IconName.Folder => "folder",
                    IconName.File => "file",
                    IconName.FileText => "file-text",
                    IconName.Box => "box",
                    IconName.Layers => "layers",
                    IconName.Link => "link",
                    IconName.Branch => "git-branch",
                    IconName.User => "user",
                    IconName.LogOut => "log-out",
                    IconName.Undo => "rotate-ccw",
                    IconName.Ban => "slash",
                    IconName.Target => "target",
                    IconName.Refresh => "refresh-cw",
                    IconName.Transfer => "repeat",
                    IconName.Download => "download",
                    IconName.Upload => "upload",
                    IconName.UploadCloud => "upload-cloud",
                    IconName.Database => "database",
                    IconName.Grid => "grid",
                    IconName.Columns => "columns",
                    IconName.Copy => "copy",
                    IconName.Eye => "eye",
                    IconName.EyeOff => "eye-off",
                    IconName.ChevronLeft => "chevron-left",
                    IconName.ChevronRight => "chevron-right",
                    IconName.ChevronUp => "chevron-up",
                    IconName.ChevronDown => "chevron-down",
                    IconName.ArrowUp => "arrow-up",
                    IconName.ArrowDown => "arrow-down",
                    IconName.ArrowLeft => "arrow-left",
                    IconName.ArrowRight => "arrow-right",
                    IconName.Activity => "activity",
                    IconName.Book => "book",
                    IconName.Layout => "layout",
                    IconName.Package => "package",
                    IconName.PieChart => "pie-chart",
                    IconName.Server => "server",
                    IconName.Share => "share-2",
                    IconName.Smile => "smile",
                    IconName.List => "list",
                    _ => "help-circle"
                };
            }
        }
    }
}
