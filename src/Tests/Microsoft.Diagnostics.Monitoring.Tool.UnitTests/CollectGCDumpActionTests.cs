﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.TestCommon;
using Microsoft.Diagnostics.Monitoring.TestCommon.Options;
using Microsoft.Diagnostics.Monitoring.TestCommon.Runners;
using Microsoft.Diagnostics.Monitoring.WebApi;
using Microsoft.Diagnostics.Tools.Monitor.CollectionRules;
using Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Actions;
using Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Exceptions;
using Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Options.Actions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Diagnostics.Monitoring.Tool.UnitTests
{
    public sealed class CollectGCDumpActionTests
    {
        private const string DefaultRuleName = "GCDumpTestRule";

        readonly private ITestOutputHelper _outputHelper;
        private readonly EndpointUtilities _endpointUtilities;

        public CollectGCDumpActionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _endpointUtilities = new(_outputHelper);
        }

        [Theory]
        [MemberData(nameof(ActionTestsHelper.GetTfms), MemberType = typeof(ActionTestsHelper))]
        public Task CollectGCDumpAction_Success(TargetFrameworkMoniker tfm)
        {
            return Retry(() => CollectGCDumpAction_SuccessCore(tfm));
        }

        private async Task CollectGCDumpAction_SuccessCore(TargetFrameworkMoniker tfm)
        {
            using TemporaryDirectory tempDirectory = new(_outputHelper);

            await TestHostHelper.CreateCollectionRulesHost(_outputHelper, rootOptions =>
            {
                rootOptions.AddFileSystemEgress(ActionTestsConstants.ExpectedEgressProvider, tempDirectory.FullName);

                rootOptions.CreateCollectionRule(DefaultRuleName)
                    .AddCollectGCDumpAction(ActionTestsConstants.ExpectedEgressProvider)
                    .SetStartupTrigger();
            }, async host =>
            {
                CollectGCDumpOptions options = ActionTestsHelper.GetActionOptions<CollectGCDumpOptions>(host, DefaultRuleName);

                ICollectionRuleActionFactoryProxy factory;
                Assert.True(host.Services.GetService<ICollectionRuleActionOperations>().TryCreateFactory(KnownCollectionRuleActions.CollectGCDump, out factory));

                EndpointInfoSourceCallback callback = new(_outputHelper);
                await using ServerSourceHolder sourceHolder = await _endpointUtilities.StartServerAsync(callback);

                await using AppRunner runner = _endpointUtilities.CreateAppRunner(sourceHolder.TransportName, tfm);

                Task<IEndpointInfo> newEndpointInfoTask = callback.WaitAddedEndpointInfoAsync(runner, CommonTestTimeouts.StartProcess);

                await runner.ExecuteAsync(async () =>
                {
                    IEndpointInfo endpointInfo = await newEndpointInfoTask;

                    ICollectionRuleAction action = factory.Create(endpointInfo, options);

                    CollectionRuleActionResult result = await ActionTestsHelper.ExecuteAndDisposeAsync(action, CommonTestTimeouts.GCDumpTimeout);

                    string egressPath = ActionTestsHelper.ValidateEgressPath(result);

                    using FileStream gcdumpStream = new(egressPath, FileMode.Open, FileAccess.Read);
                    Assert.NotNull(gcdumpStream);

                    await ValidateGCDump(gcdumpStream);

                    await runner.SendCommandAsync(TestAppScenarios.AsyncWait.Commands.Continue);
                });
            });
        }

        private static async Task ValidateGCDump(Stream gcdumpStream)
        {
            using CancellationTokenSource cancellation = new(CommonTestTimeouts.GCDumpTimeout);
            byte[] buffer = await gcdumpStream.ReadBytesAsync(24, cancellation.Token);

            const string knownHeaderText = "!FastSerialization.1";

            Encoding enc8 = Encoding.UTF8;

            string headerText = enc8.GetString(buffer, 4, knownHeaderText.Length);

            Assert.Equal(knownHeaderText, headerText);
        }

        private async Task Retry(Func<Task> func, int attemptCount = 3)
        {
            int attemptIteration = 0;
            while (true)
            {
                attemptIteration++;
                _outputHelper.WriteLine("===== Attempt #{0} =====", attemptIteration);
                try
                {
                    await func();

                    break;
                }
                catch (CollectionRuleActionException ex) when (attemptIteration < attemptCount && ex.InnerException is InvalidOperationException)
                {
                    // GC dumps can fail to be produced from the runtime because the pipeline doesn't get the expected
                    // start, data, and stop events. The pipeline will throw an InvalidOperationException, which is
                    // wrapped in a CollectionRuleActionException by the action. Allow retries when this occurs.
                }
            }
        }
    }
}
