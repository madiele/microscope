#nullable enable

namespace Microscope.Shared {
    public class CodeLensDetails {
        public CodeLensDetails(CodeLensDetailsData codeLensDetailsData) {
            CodeLensDetailsData = codeLensDetailsData;
        }

        public CodeLensDetailsData CodeLensDetailsData { get; set; }
    }
}
