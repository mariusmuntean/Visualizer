using GraphQL.Types;
using Redis.OM.Modeling;

namespace Visualizer.API.GraphQl.Types.Tweet;

public class GeoLocTypeQl : ObjectGraphType<GeoLoc>
{
    public GeoLocTypeQl()
    {
        Field(loc => loc.Latitude, false, typeof(FloatGraphType));
        Field(loc => loc.Longitude, false, typeof(FloatGraphType));
    }
}
