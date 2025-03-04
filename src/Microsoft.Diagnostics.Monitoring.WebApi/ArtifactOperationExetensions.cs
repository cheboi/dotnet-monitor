﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.WebApi
{
    internal static class ArtifactOperationExetensions
    {
        public static Task ExecuteAsync(this IArtifactOperation operation, Stream outputStream, CancellationToken token)
        {
            return operation.ExecuteAsync(outputStream, startCompletionSource: null, token);
        }
    }
}
