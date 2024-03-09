#nullable enable

namespace Microscope.Shared {
    public class AnalysisResult {
        public AnalysisResult(string parentName, string symbolName) {
            ParentName = parentName;
            SymbolName = symbolName;
        }

        public string ParentName { get; }
        public string SymbolName { get; }
    }
}
