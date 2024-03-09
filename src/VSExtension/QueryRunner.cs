#nullable enable

namespace Microscope.VSExtension {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media.Media3D;

    using Microscope.CodeAnalysis;
    using Microscope.Shared;
    using Microscope.VSExtension.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.VisualStudio.Language.CodeLens;
    using Microsoft.VisualStudio.LanguageServices;
    using Microsoft.VisualStudio.Utilities;

    using Newtonsoft.Json;

    using static Microscope.Shared.Logging;

    [Export(typeof(ICodeLensCallbackListener))]
    [ContentType("CSharp")]
    [ContentType("Basic")]
    public partial class QueryRunner : ICodeLensCallbackListener, IQueryRunner {
        private readonly VisualStudioWorkspace workspace;

        [ImportingConstructor]
        public QueryRunner(VisualStudioWorkspace workspace) => this.workspace = workspace;

        public async Task<bool> IsExtensionEnabled() {
            var opts = await GeneralOptions.GetLiveInstanceAsync().Caf();
            return opts.Enabled;
        }

        public int GetVisualStudioPid() => Process.GetCurrentProcess().Id;

        public async Task<CodeLensHeaderData> RunQueryForHeader(
            Guid dataPointId,
            Guid projGuid,
            string filePath,
            int textStart,
            int textLen,
            CancellationToken ct) {
            try {
                var document = workspace.GetDocument(filePath, projGuid);
                var symbol = await document.GetSymbolAt(new TextSpan(textStart, textLen), ct).Caf();

                var analysisResult = symbol switch {
                    IMethodSymbol method => new AnalysisResult(method.ContainingSymbol.Name,
                                                               method.Name),
                    IParameterSymbol parameter => new AnalysisResult(parameter.ContainingSymbol.Name,
                                                               parameter.Name),
                    _ => throw new NotImplementedException(),
                };

                return new CodeLensHeaderData(analysisResult);
            } catch (Exception ex) {
                LogVS(ex);
                return new CodeLensHeaderData();
            }
        }

        public async Task<CodeLensDetailsData> RunQueryForDetails(Guid dataPointId, Guid projGuid, string filePath, int textStart, int textLen, CancellationToken ct) {
            try {
                var document = workspace.GetDocument(filePath, projGuid);
                var symbol = await document.GetSymbolAt(new TextSpan(textStart, textLen), ct).Caf();

                var result = symbol switch {
                    IMethodSymbol method => new AnalysisResult(method.ContainingSymbol.Name,
                                                               method.Name),
                    IParameterSymbol parameter => new AnalysisResult(parameter.ContainingSymbol.Name,
                                                               parameter.Name),
                    _ => throw new NotImplementedException(),
                };

                var client = new HttpClient();

                var request = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api.loganalytics.azure.com/v1/workspaces/DEMO_WORKSPACE/query"),
                    Headers =
                    {
                { "X-Api-Key", "DEMO_KEY" },
            },
                    Content = new StringContent("{\"query\": \"print ResultCode=200, Count=2000 | union (print ResultCode=404, Count=1000) | union (print ResultCode=500, Count=5)\"}", Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(request)) {
                    _ = response.EnsureSuccessStatusCode();
                    var body = JsonConvert.DeserializeObject<Root>(await response.Content.ReadAsStringAsync());
                    return new CodeLensDetailsData(body.tables[0].rows);
                }
            } catch (Exception ex) {
                LogVS(ex);
                return new CodeLensDetailsData();
            }
        }
    }
}
