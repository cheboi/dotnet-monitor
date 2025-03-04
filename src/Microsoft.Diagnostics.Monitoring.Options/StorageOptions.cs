﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.Options;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Diagnostics.Monitoring.WebApi
{
    internal class StorageOptions
    {
        [Display(
            ResourceType = typeof(OptionsDisplayStrings),
            Description = nameof(OptionsDisplayStrings.DisplayAttributeDescription_StorageOptions_DefaultSharedPath))]
        public string DefaultSharedPath { get; set; }

        [Display(
            ResourceType = typeof(OptionsDisplayStrings),
            Description = nameof(OptionsDisplayStrings.DisplayAttributeDescription_StorageOptions_DumpTempFolder))]
        public string DumpTempFolder { get; set; }

        [Experimental]
        [Display(
            ResourceType = typeof(OptionsDisplayStrings),
            Description = nameof(OptionsDisplayStrings.DisplayAttributeDescription_StorageOptions_SharedLibraryPath))]
        public string SharedLibraryPath { get; set; }
    }
}
