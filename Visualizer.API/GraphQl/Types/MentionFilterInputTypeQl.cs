using GraphQL.Types;
using Visualizer.API.Services.Ingestion;

namespace Visualizer.GraphQl.Types;

public class MentionFilterInputTypeQl : InputObjectGraphType<TweetGraphService.MentionFilterDto>
{
    public MentionFilterInputTypeQl()
    {
        Field(dto => dto.AuthorUserName, true, typeof(StringGraphType));
        Field(dto => dto.MentionedUserNames, true, typeof(ListGraphType<StringGraphType>));
        Field(dto => dto.Amount, true, typeof(IntGraphType));
        Field(dto => dto.MinHops, true, typeof(IntGraphType));
        Field(dto => dto.MaxHops, true, typeof(IntGraphType));
    }
}