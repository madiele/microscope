#nullable enable

namespace Microscope.Shared {
    public class CodeLensHeaderData {
        public string Description { get; set; }
        public string Tooltip { get; set; }
        public int Count { get; set; }
        public CodeLensHeaderData() {
            Description = string.Empty;
            Tooltip = string.Empty;
            Count = 0;
        }

        public CodeLensHeaderData(AnalysisResult analysisResult) {
            Tooltip = $"{analysisResult.ParentName}";
            Description = $"{analysisResult.ParentName}.{ analysisResult.SymbolName}";
            Count = 1;
        }
    }
}
