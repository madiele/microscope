﻿#nullable enable

namespace Microscope.CodeLensProvider {
    using System;
    using System.ComponentModel.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microscope.Shared;

    using Microsoft.VisualStudio.Language.CodeLens;
    using Microsoft.VisualStudio.Language.CodeLens.Remoting;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Utilities;

    using static Microscope.Shared.Logging;

    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(ProviderId)]
    [LocalizedName(typeof(Resources), "Name")]
    [ContentType("CSharp")]
    [ContentType("Basic")]
    [Priority(210)] // sort after "references" CodeLens (200)
    public class CodeLensProvider : IAsyncCodeLensDataPointProvider {
        public const string ProviderId = "ILInstructions";
        private readonly Lazy<ICodeLensCallbackService> callbackService;

        [ImportingConstructor]
        public CodeLensProvider(Lazy<ICodeLensCallbackService> callbackService) {
            this.callbackService = callbackService;
            LogCL(); // logs the PID of the out-of-process CodeLens engine
        }

        public async Task<bool> CanCreateDataPointAsync(
            CodeLensDescriptor descriptor,
            CodeLensDescriptorContext context,
            CancellationToken ct)
            => (descriptor.Kind is CodeElementKinds.Method || descriptor.Kind is CodeElementKinds.Property)
            && await callbackService.Value
                .InvokeAsync<bool>(this, nameof(IQueryRunner.IsExtensionEnabled)).Caf();

        public async Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(
            CodeLensDescriptor descriptor,
            CodeLensDescriptorContext context,
            CancellationToken ct) {
            try {
                var dp = new CodeLensDataPoint(callbackService.Value, descriptor);

                var vspid = await callbackService.Value
                    .InvokeAsync<int>(this, nameof(IQueryRunner.GetVisualStudioPid)).Caf();
                await dp.ConnectToVisualStudio(vspid).Caf();

                return dp;
            } catch (Exception ex) {
                LogCL(ex);
                throw;
            }
        }
    }
}
