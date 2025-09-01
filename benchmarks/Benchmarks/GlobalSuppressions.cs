using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmark methods should remain instance methods for proper BenchmarkDotNet operation")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Benchmark code uses default formatting for performance measurement purposes")]
[assembly: SuppressMessage("Naming", "CA1707:Remove the underscores from member name", Justification = "Benchmark method names use underscores for clear scenario identification and readability")]