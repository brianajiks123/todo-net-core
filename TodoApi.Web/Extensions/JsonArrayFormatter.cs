using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Text;
using System.Text.Json;

namespace TodoApi.Web.Extensions;

public sealed class JsonArrayFileSink : ILogEventSink
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private const string Closing = "\n]";

    private readonly string _logDirectory;
    private readonly string _filePrefix;
    private readonly object _lock = new();

    private string? _currentFilePath;
    private StreamWriter? _writer;

    public JsonArrayFileSink(string logDirectory = "logs", string filePrefix = "log-")
    {
        _logDirectory = logDirectory;
        _filePrefix = filePrefix;
    }

    public void Emit(LogEvent logEvent)
    {
        var filePath = GetFilePath(logEvent.Timestamp);
        var entry = JsonSerializer.Serialize(BuildEntry(logEvent), _jsonOptions);
        var indented = string.Join("\n", entry.Split('\n').Select(l => "  " + l));

        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(_logDirectory);
                EnsureWriter(filePath);

                if (_writer!.BaseStream.Length == 0)
                {
                    // First entry: open array
                    _writer.Write("[\n" + indented + Closing);
                }
                else
                {
                    // Subsequent entries: seek back over closing ] and append
                    _writer.Flush();
                    var stream = _writer.BaseStream;
                    var closingBytes = Encoding.UTF8.GetByteCount(Closing);
                    stream.Seek(-closingBytes, SeekOrigin.End);
                    stream.SetLength(stream.Position);
                    _writer.Write(",\n" + indented + Closing);
                }

                _writer.Flush();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[JsonArrayFileSink ERROR] {ex.Message}");
            }
        }
    }

    private void EnsureWriter(string filePath)
    {
        if (_currentFilePath == filePath && _writer != null)
            return;

        // Rolling: close old writer, open new file
        _writer?.Flush();
        _writer?.Dispose();

        _currentFilePath = filePath;
        _writer = new StreamWriter(
            new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read),
            Encoding.UTF8,
            leaveOpen: false);

        // If file already has content from a previous run, position at end
        _writer.BaseStream.Seek(0, SeekOrigin.End);
    }

    private string GetFilePath(DateTimeOffset timestamp) =>
        Path.Combine(_logDirectory, $"{_filePrefix}{timestamp:yyyyMMdd}.json");

    private static Dictionary<string, object?> BuildEntry(LogEvent logEvent)
    {
        var entry = new Dictionary<string, object?>
        {
            ["Timestamp"] = logEvent.Timestamp.ToString("o"),
            ["Level"]     = logEvent.Level.ToString(),
            ["Message"]   = logEvent.RenderMessage(),
        };

        if (logEvent.Exception is not null)
            entry["Exception"] = logEvent.Exception.ToString();

        if (logEvent.Properties.Count > 0)
        {
            entry["Properties"] = logEvent.Properties
                .ToDictionary(
                    p => p.Key,
                    p => (object?)RenderValue(p.Value));
        }

        return entry;
    }

    private static object? RenderValue(LogEventPropertyValue value) => value switch
    {
        ScalarValue sv       => sv.Value,
        SequenceValue seq    => seq.Elements.Select(RenderValue).ToList(),
        StructureValue str   => str.Properties.ToDictionary(p => p.Name, p => RenderValue(p.Value)),
        DictionaryValue dict => dict.Elements.ToDictionary(
                                    kv => kv.Key.Value?.ToString() ?? "",
                                    kv => RenderValue(kv.Value)),
        _                    => value.ToString()
    };
}
