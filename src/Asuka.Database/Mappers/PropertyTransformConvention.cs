using System.Text.RegularExpressions;
using Dapper.FluentMap.Conventions;

namespace Asuka.Database.Mappers
{
    /// <summary>
    /// Convention to map PascalCase properties to lower_snake_case columns.
    /// </summary>
    public class PropertyTransformConvention : Convention
    {
        public PropertyTransformConvention()
        {
            Properties()
                .Configure(c => c.Transform(s =>
                    Regex
                        .Replace(s,
                        "([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])",
                        "$1$3_$2$4")
                        .ToLower()));
        }
    }
}
