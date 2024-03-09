#nullable enable

namespace Microscope.Shared {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueryRunner {
        Task<bool> IsExtensionEnabled();

        int GetVisualStudioPid();

        Task<CodeLensHeaderData> RunQueryForHeader(Guid dataPointId, Guid projGuid, string filePath, int textStart, int textLen, CancellationToken ct);
        Task<CodeLensDetailsData> RunQueryForDetails(Guid dataPointId, Guid projGuid, string filePath, int textStart, int textLen, CancellationToken ct);
    }
}
