using GraphQL.Types;
using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.GraphQl.Types.Input;

public class GeoFilterInputTypeQl : InputObjectGraphType<GeoFilter>
{
    public GeoFilterInputTypeQl()
    {
        Field(filter => filter.Latitude, false, typeof(FloatGraphType));
        Field(filter => filter.Longitude, false, typeof(FloatGraphType));
        Field(filter => filter.RadiusKm, false, typeof(FloatGraphType));
    }
}
