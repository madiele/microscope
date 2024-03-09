#nullable enable

namespace Microscope.Shared {
    using System.Collections.Generic;

    public struct CodeLensDetailsData {
        public CodeLensDetailsData(dynamic[][] rows) {
            Rows = rows;
        }

        public dynamic[][] Rows { get; }
    }
    public struct Column {
        public string name { get; set; }
        public string type { get; set; }
    }

    public struct Table {
        public string name { get; set; }
        public List<Column> columns { get; set; }
        public dynamic[][] rows { get; set; }
    }

    public struct Root {
        public List<Table> tables { get; set; }
    }
}
