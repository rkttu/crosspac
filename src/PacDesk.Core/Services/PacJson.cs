using System.Text.Json;

namespace PacDesk.Core.Services;

/// <summary>
/// Tolerant helpers for reading pac's <c>--json</c> output. Property names are matched
/// case-insensitively and several candidate names are accepted, because the exact schema
/// varies by pac version (and, for verbs that don't yet emit JSON, isn't observable at
/// all). Values are cloned so they remain valid after the parsed document is disposed.
/// </summary>
internal static class PacJson
{
    public static IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> ParseObjects(string json)
    {
        var list = new List<IReadOnlyDictionary<string, JsonElement>>();
        if (string.IsNullOrWhiteSpace(json))
            return list;

        using var document = JsonDocument.Parse(json);

        // Accept either a bare array, or an object wrapping an array (e.g. { "value": [...] }).
        JsonElement array;
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            array = root;
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            var wrapped = root.EnumerateObject()
                .FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);
            if (wrapped.Value.ValueKind != JsonValueKind.Array)
                return list;
            array = wrapped.Value;
        }
        else
        {
            return list;
        }

        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
                continue;

            var map = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in element.EnumerateObject())
                map[property.Name] = property.Value.Clone();
            list.Add(map);
        }

        return list;
    }

    public static string Str(IReadOnlyDictionary<string, JsonElement> map, params string[] names)
    {
        foreach (var name in names)
        {
            if (!map.TryGetValue(name, out var value))
                continue;
            return value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : value.ToString();
        }
        return "";
    }

    public static bool Bool(IReadOnlyDictionary<string, JsonElement> map, params string[] names)
    {
        foreach (var name in names)
        {
            if (!map.TryGetValue(name, out var value))
                continue;
            switch (value.ValueKind)
            {
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.String:
                    var s = value.GetString() ?? "";
                    return s.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || s.Equals("yes", StringComparison.OrdinalIgnoreCase)
                        || s.Equals("managed", StringComparison.OrdinalIgnoreCase);
            }
        }
        return false;
    }
}
