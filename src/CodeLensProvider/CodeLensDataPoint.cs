#nullable enable

namespace Microscope.CodeLensProvider {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microscope.Shared;
    using Microsoft.VisualStudio.Language.CodeLens;
    using Microsoft.VisualStudio.Language.CodeLens.Remoting;
    using Microsoft.VisualStudio.Threading;
    using static Microscope.Shared.Logging;

    public class CodeLensDataPoint : IAsyncCodeLensDataPoint, IDisposable {
        private static readonly CodeLensDetailEntryCommand refreshCmdId = new CodeLensDetailEntryCommand {
            // Defined in file `src/VSExtension/MicroscopeCommands.vsct`.
            CommandSet = new Guid("32872e4d-3d0e-4b26-9ef8-d3a90080429f"),
            CommandId = 0x0100
        };

        public readonly Guid _datapointId = Guid.NewGuid();
        private readonly ICodeLensCallbackService _callbackService;
        private VisualStudioConnectionHandler? _visualStudioConnection;

        public CodeLensDescriptor Descriptor { get; }

        public event AsyncEventHandler? InvalidatedAsync;

        public CodeLensDataPoint(ICodeLensCallbackService callbackService, CodeLensDescriptor descriptor) {
            _callbackService = callbackService;
            Descriptor = descriptor;
        }

        public void Dispose() => _visualStudioConnection?.Dispose();

        public async Task ConnectToVisualStudio(int vspid) =>
            _visualStudioConnection = await VisualStudioConnectionHandler.Create(owner: this, vspid).Caf();

        /// <summary>
        /// chiamata quando il metodo è in vista mentre si aspetta mostra un loader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext context, CancellationToken cancellationToken) {
            try {
                var data = await RunQueryForHeader(context,
                                               _datapointId,
                                               Descriptor.ProjectGuid,
                                               Descriptor.FilePath,
                                               cancellationToken).Caf();

                return new CodeLensDataPointDescriptor {
                    Description = data.Description,
                    TooltipText = data.Tooltip,
                    ImageId = null,
                    IntValue = data.Count,
                };
            } catch (Exception ex) {
                LogCL(ex);
                throw;
            }
        }

        /// <summary>
        /// chiamata quando si espande il lens
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext context, CancellationToken ct) {
            try {
                // When opening the details pane, the data point is re-created leaving `data` uninitialized. VS will
                // then call `GetDataAsync()` and `GetDetailsAsync()` concurrently.
                var data = await RunQueryForDetails(context,
                                                   _datapointId,
                                                   Descriptor.ProjectGuid,
                                                   Descriptor.FilePath,
                                                   ct).Caf();


                // questo descrive cosa viene mostrato nel pannello
                return new CodeLensDetailsDescriptor {
                    // Since it's impossible to figure out how to use [DetailsTemplateName], we'll
                    // just use the default grid template without any headers/entries and add
                    // what we want to transmit to the custom data.
                    Headers = Enumerable.Empty<CodeLensDetailHeaderDescriptor>(),
                    Entries = Enumerable.Empty<CodeLensDetailEntryDescriptor>(),
                    CustomData = new[] { new CodeLensDetails(data!) }, //CodeLensDetail usa id per prendere i dati dalla cache
                    PaneNavigationCommands = new[] {
                        new CodeLensDetailPaneCommand {
                            CommandDisplayName = "Refresh",
                            CommandId = refreshCmdId,
                            CommandArgs = new[] { (object)_datapointId }
                        }

                    }
                };
            } catch (Exception ex) {
                LogCL(ex);
                throw;
            }
        }

        // Called from VS via JSON RPC.
        public void Refresh() => _ = InvalidatedAsync?.InvokeAsync(this, EventArgs.Empty);
        private async Task<CodeLensHeaderData> RunQueryForHeader(CodeLensDescriptorContext ctx,
                                                          Guid datapointId,
                                                          Guid projectGuid,
                                                          string filePath,
                                                          CancellationToken ct)
            => await _callbackService
                .InvokeAsync<CodeLensHeaderData>(
                    this,
                    nameof(IQueryRunner.RunQueryForHeader),
                    new object[] { 
                        datapointId,
                        projectGuid,
                        filePath,
                        ctx.ApplicableSpan != null
                            ? ctx.ApplicableSpan.Value.Start
                            : throw new InvalidOperationException($"No ApplicableSpan given for {ctx.FullName()}."),
                        ctx.ApplicableSpan!.Value.Length
                    },
                    ct).Caf();

        private async Task<CodeLensDetailsData> RunQueryForDetails(CodeLensDescriptorContext ctx,
                                                          Guid datapointId,
                                                          Guid projectGuid,
                                                          string filePath,
                                                          CancellationToken ct)
            => await _callbackService
                .InvokeAsync<CodeLensDetailsData>(
                    this,
                    nameof(IQueryRunner.RunQueryForDetails),
                    new object[] { 
                        datapointId,
                        projectGuid,
                        filePath,
                        ctx.ApplicableSpan != null
                            ? ctx.ApplicableSpan.Value.Start
                            : throw new InvalidOperationException($"No ApplicableSpan given for {ctx.FullName()}."),
                        ctx.ApplicableSpan!.Value.Length
                    },
                    ct).Caf();
    }
}
