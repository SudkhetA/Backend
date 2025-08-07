using System.Text.RegularExpressions;

namespace Backend.Utilities.Helper;

public partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is not null )
        {
            // Slugify value
            return MyRegex().Replace(value.ToString() ?? string.Empty, "$1-$2").ToLower(System.Globalization.CultureInfo.CurrentCulture);
        }
        return null;
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex MyRegex();
}