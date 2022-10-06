using GraphQL.Types;
using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.GraphQl.Types.Input;

public class FindTweetsInputTypeQl : InputObjectGraphType<FindTweetsInputDto>
{
    public FindTweetsInputTypeQl()
    {
        Field(dto => dto.TweetId, true, typeof(StringGraphType));
        Field(dto => dto.AuthorId, true, typeof(StringGraphType));
        Field(dto => dto.Username, true, typeof(StringGraphType));
        Field(dto => dto.SearchTerm, true, typeof(StringGraphType));
        Field(dto => dto.Hashtags, true, typeof(ListGraphType<StringGraphType>));

        Field(dto => dto.OnlyWithGeo, true, typeof(BooleanGraphType));

        Field(dto => dto.StartingFrom, true, typeof(DateTimeGraphType));
        Field(dto => dto.UpTo, true, typeof(DateTimeGraphType));

        Field(dto => dto.PageSize, true, typeof(IntGraphType));
        Field(dto => dto.PageNumber, true, typeof(IntGraphType));

        Field(dto => dto.SortField, true, typeof(EnumerationGraphType<SortField>));
        Field(dto => dto.SortOrder, true, typeof(EnumerationGraphType<SortOrder>));
    }
}
