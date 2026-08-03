using CineMatch.Api.Enums;

namespace CineMatch.Api.Data.Contracts
{
    public class SearchResult
    {
        public int TmdbId { get; set; }
        public ContentType Type { get; set; }
    }
}
