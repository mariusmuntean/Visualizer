using GraphQL.Types;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Types;

public class MentionFilterInputTypeQl : InputObjectGraphType<TweetGraphService.MentionFilterDto>
{
    public MentionFilterInputTypeQl()
    {
        Field(dto => dto.AuthorUserName, true, typeof(StringGraphType));
        Field(dto => dto.MentionedUserNames, true, typeof(ListGraphType<StringGraphType>));
        Field(dto => dto.Amount, true, typeof(IntGraphType));
    }
}