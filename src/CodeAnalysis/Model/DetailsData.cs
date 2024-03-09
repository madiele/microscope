#nullable enable

namespace Microscope.CodeAnalysis.Model {
    using Microscope.Shared;
    using System.Linq;

    using System.Collections.Generic;
    using System.Collections.Immutable;

    public struct TableRow {
        public TableRow(string responseCode, string count) {
            ResponseCode = responseCode;
            Count = count;
        }

        public string ResponseCode { get; }
        public string Count { get; }
    }
    //questo definisce il pannello con griglia dei dettagli
    public readonly struct DetailsData {
        public IReadOnlyList<TableRow>? TableRows { get; }

        public DetailsData(CodeLensDetails tableRows) 
            => TableRows = (IReadOnlyList<TableRow>?)tableRows
                             .CodeLensDetailsData.Rows
                             .Select(x => new TableRow($"{x[0]}",
                                                       $"{x[1]}")).ToImmutableList();
    }
}
