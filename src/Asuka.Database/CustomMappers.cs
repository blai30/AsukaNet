using Asuka.Database.Mappers;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;

namespace Asuka.Database
{
    /// <summary>
    /// Set up dapper mappers for POCO properties to columns.
    /// </summary>
    public static class CustomMappers
    {
        public static void Initialize()
        {
            // Configure mapping to snake_case tables and columns.
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            // Configure Dapper.FluentMap conventions.
            FluentMapper.Initialize(config =>
            {
                config.AddConvention<PropertyTransformConvention>()
                    .ForEntitiesInCurrentAssembly("Asuka.Database.Models");

                // Entity maps.
                config.AddMap(new TagMap());

                // Dommel mappers.
                config.ForDommel();
            });
        }
    }
}
