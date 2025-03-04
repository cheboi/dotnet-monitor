﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.EventPipe;
using Microsoft.Diagnostics.Monitoring.Options;
using Microsoft.Diagnostics.Monitoring.WebApi;
using Microsoft.Extensions.Logging;

namespace Microsoft.Diagnostics.Tools.Monitor
{
    internal sealed class LogsOperationFactory : ILogsOperationFactory
    {
        private readonly ILogger<LogsOperation> _logger;

        public LogsOperationFactory(ILogger<LogsOperation> logger)
        {
            _logger = logger;
        }

        public IArtifactOperation Create(IEndpointInfo endpointInfo, EventLogsPipelineSettings settings, LogFormat format)
        {
            return new LogsOperation(endpointInfo, settings, format, _logger);
        }
    }
}
