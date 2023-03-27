using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker;

public record FontLayout
{
    public record AtlasData(double Size, int Width, int Height);

    public record AtlasBounds(double Left, double Bottom, double Right, double Top);

    public record Glyph(int Unicode, double Advance, PlaneBounds? PlaneBounds, AtlasBounds? AtlasBounds);

    public record MetricsData(double Ascender, double Descender);

    public record PlaneBounds(double Left, double Top);

    [JsonInclude]
    public AtlasData? Atlas { get; private set; }

    [JsonInclude]
    public MetricsData? Metrics { get; private set; }

    [JsonInclude]
    public IImmutableList<Glyph>? Glyphs { get; private set; }
}