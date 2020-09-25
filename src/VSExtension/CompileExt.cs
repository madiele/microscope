﻿#nullable enable

namespace Microscope.VSExtension {
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microscope.Shared;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Emit;

    using Mono.Cecil;

    public static class CompileExt {
        public static async Task<AssemblyDefinition?> Compile(this Project proj, Stream peStream, CancellationToken ct) {
            var compilation = await proj.GetCompilationAsync(ct).Caf()
                ?? throw new InvalidOperationException($"Project {proj.FilePath} does not support compilation.");

            var result = compilation.Emit(peStream);
            if (!result.Success) return null;

            _ = peStream.Seek(0, SeekOrigin.Begin);
            return AssemblyDefinition.ReadAssembly(peStream);
        }

        private static string PrintErrors(this EmitResult result) {
            var errs = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            return string.Join("\n", errs);
        }
    }
}
