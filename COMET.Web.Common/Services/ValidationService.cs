// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValidationService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Services
{
    using CDP4Common.CommonData;

    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The purpose of the <see cref="ValidationService" /> is to check and report on the validity of a field in a
    /// </summary>
    public static class ValidationService
    {
        /// <summary>
        /// The validation map provides the mapping between field names and <see cref="ValidationRule" />s.
        /// </summary>
        private static readonly Dictionary<string, ValidationRule> ValidationMap = new()
        {
            { "SelectedSource", new ValidationRule { PropertyName = "SelectedSource", Rule = @"^(?!\s*$).+", ErrorText = "The SelectedSource must not be empty." } },
            { "ShortName", new ValidationRule { PropertyName = "ShortName", Rule = @"^[a-zA-Z][a-zA-Z0-9_]*$", ErrorText = "The ShortName must start with a letter and not contain any spaces or non alphanumeric characters." } },
            { "RDLShortName", new ValidationRule { PropertyName = "ShortName", Rule = @"^([^()\s][\S\s]*)$", ErrorText = "The ShortName can not be empty or start with a whitespace." } },
            { "RDLName", new ValidationRule { PropertyName = "Name", Rule = @"^([^()\s][\S\s]*)$", ErrorText = "The Name can not be empty or start with a whitespace." } },
            { "NativeName", new ValidationRule { PropertyName = "NativeName", Rule = @"^[a-zA-Z][a-zA-Z0-9_]*$", ErrorText = "The NativeName must start with a letter and not contain any spaces or non alphanumeric characters." } },
            { "FileRevisionName", new ValidationRule { PropertyName = "Name", Rule = @"^([^()\s][\S\s]*)$", ErrorText = "The Name can not be empty or start with a whitespace." } },
            { "Name", new ValidationRule { PropertyName = "Name", Rule = @"^(\p{L}|\p{L}[^()]*[^()\s])$", ErrorText = "The Name must start with a letter and not contain any parentheses or trailing spaces." } },
            { "EmailAddress", new ValidationRule { PropertyName = "EmailAddress", Rule = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorText = "The Email must follow the RFC 5321 protocol." } },
            { "TelephoneNumber", new ValidationRule { PropertyName = "TelephoneNumber", Rule = @"^(?!\s*$).+", ErrorText = "The Telephone Number must not be empty." } },
            { "UserPreference", new ValidationRule { PropertyName = "UserPreference", Rule = @"^(?!\s*$).+", ErrorText = "The User Preference must not be empty." } },
            { "LanguageCode", new ValidationRule { PropertyName = "LanguageCode", Rule = @"^(?!\s*$).+", ErrorText = "The Language Code must not be empty." } },
            { "Content", new ValidationRule { PropertyName = "Content", Rule = @"^(?!\s*$).+", ErrorText = "The Content must not be empty." } },
            { "Password", new ValidationRule { PropertyName = "Password", Rule = @"^(?!\s*$).+", ErrorText = "The Password must not be empty." } },
            { "ForwardRelationshipName", new ValidationRule { PropertyName = "ForwardRelationshipName", Rule = @"^(?!\s*$).+", ErrorText = "The Forward Relationship Name must not be empty." } },
            { "InverseRelationshipName", new ValidationRule { PropertyName = "InverseRelationshipName", Rule = @"^(?!\s*$).+", ErrorText = "The Inverse Relationship Name must not be empty." } },
            { "Exponent", new ValidationRule { PropertyName = "Exponent", Rule = @"^(?!\s*$).+", ErrorText = "The Exponent must not be empty." } },
            { "Symbol", new ValidationRule { PropertyName = "Symbol", Rule = @"^(?!\s*$).+", ErrorText = "The Symbol must not be empty." } },
            { "ScaleValueDefinition", new ValidationRule { PropertyName = "ScaleValueDefinition", Rule = @"^(?!\s*$).+", ErrorText = "The Value must not be empty." } },
            { "ScaleReferenceQuantityValue", new ValidationRule { PropertyName = "ScaleReferenceQuantityValue", Rule = @"^(?!\s*$).+", ErrorText = "The Value must not be empty." } },
            { "Factor", new ValidationRule { PropertyName = "Factor", Rule = @"^(?!\s*$).+", ErrorText = "The factor must not be empty." } },
            { "Modulus", new ValidationRule { PropertyName = "Modulus", Rule = @"^(?!\s*$).+", ErrorText = "The Modulus must not be empty." } },
            { "Extension", new ValidationRule { PropertyName = "Extension", Rule = @"^[a-z0-9]+$", ErrorText = "The Extension shall only contain lower cases or digits and cannot be empty." } },
            { "FileTypeName", new ValidationRule { PropertyName = "FileTypeName", Rule = @"^(application|audio|example|image|message|model|multipart|text|video)/\S+$", ErrorText = "The Name or short-name shall be consistent with the IANA standard and starts with either \"application/\", \"audio/\", \"example/\", \"image/\", \"message/\", \"model/\", \"multipart/\", \"text/\" or \"video/\" followed by any characters except white-spaces." } },
            { "Value", new ValidationRule { PropertyName = "Value", Rule = @"^(?!\s*$).+", ErrorText = "The Value must not be empty." } },
            { "ModelSetupShortName", new ValidationRule { PropertyName = "ShortName", Rule = @"^[a-zA-Z0-9_]*$", ErrorText = "The ShortName shall only contain alpha-numerical character." } },
            { "PersonShortName", new ValidationRule { PropertyName = "Value", Rule = @"^(?!\s*$).+", ErrorText = "The User Name must not be empty." } },
            { "PersonGivenName", new ValidationRule { PropertyName = "Value", Rule = @"^(?!\s*$).+", ErrorText = "The Given Name must not be empty." } },
            { "PersonSurname", new ValidationRule { PropertyName = "Value", Rule = @"^(?!\s*$).+", ErrorText = "The Surname must not be empty." } },
            { "RequirementShortName", new ValidationRule { PropertyName = "Value", Rule = @"^\w+[\w-]*$", ErrorText = "The Requirement ShortName cannot be empty and must contain only alphanumeric characters and dashes." } },
            { "ModelSetupName", new ValidationRule { PropertyName = "Name", Rule = @"^([^()\s][\S\s]*)$", ErrorText = "The Name can not be empty or start with a whitespace." } },
            { "ConversionFactor", new ValidationRule { PropertyName = "ConversionFactor", Rule = @"^(?!\s*$).+", ErrorText = "The ConversionFactor must not be empty" } },
            { "Description", new ValidationRule { PropertyName = "Description", Rule = @"^(?!\s*$).+", ErrorText = "The Description must not be empty." } },
            { "Title", new ValidationRule { PropertyName = "Title", Rule = @"^(?!\s*$).+", ErrorText = "The Title must not be empty." } },
            { "EnumerationValueDefinitionShortName", new ValidationRule { PropertyName = "ShortName", Rule = @"^([^()\s][\S]*)$", ErrorText = "The ShortName can not be empty or contain a whitespace." } },
            { "EnumerationValueDefinitionName", new ValidationRule { PropertyName = "Name", Rule = @"^([^()\s][\S]*)$", ErrorText = "The Name can not be empty or contain a whitespace." } }
        };

        /// <summary>
        /// Validate all the properties of a <see cref="Thing"/> using REFLECTION!
        /// </summary>
        /// <param name="thing">the thing to validate</param>
        /// <returns>the collection of errors if any</returns>
        public static IEnumerable<string> ValidateThing(Thing thing)
        {
            var properties = thing.GetType().GetProperties();

            var validationErrors = new List<string>();

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(thing);
                var validationError = ValidateProperty(property.Name, propertyValue);
                validationErrors.Add(validationError);
            }

            return validationErrors;
        }

        /// <summary>
        /// Validate a property of a <see cref="Thing"/>
        /// </summary>
        /// <param name="propertyName">the name of the property to validate</param>
        /// <param name="value">the value to validate</param>
        /// <returns>
        /// The <see cref="string" /> with the error text, or null if there is no validation error
        /// (either because there is no rule for the given property or because the given value is correct)
        /// </returns>
        public static string ValidateProperty(string propertyName, object value)
        {
            // if no rule exists just return null in sign of ignorance
            if (!ValidationMap.TryGetValue(propertyName, out var rule))
            {
                return null;
            }

            var newValue = value == null ? string.Empty : value.ToString() ?? string.Empty;

            // get the value, if the value is null set to empty string (assume user entered no value to begin with) and check against that
            var validationPass = Regex.IsMatch(newValue, rule.Rule);

            return validationPass ? null : rule.ErrorText;
        }

        /// <summary>
        /// The validation rule contains the regex and the error texts that goes with the rule.
        /// </summary>
        public class ValidationRule
        {
            /// <summary>
            /// Gets or sets the name of the property
            /// </summary>
            public string PropertyName { get; set; }

            /// <summary>
            /// Gets or sets the rule, i.e. the Regex that must be matched for this rule to be satisfied.
            /// </summary>
            public string Rule { get; set; }

            /// <summary>
            /// Gets or sets the error text of this rule.
            /// </summary>
            public string ErrorText { get; set; }
        }
    }
}
