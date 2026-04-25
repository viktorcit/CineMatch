using CineMatch.Data.DTO;
using CineMatch.Enums;
using CineMatch.Model;

namespace CineMatch.Parsers
{
    public class HtmlParser
    {
        private readonly HttpClient _httpClient;

        public HtmlParser(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }







        //public async Task<BaseResponseWithDataDto<MovieParsedData>> GetMovieDataAsync(string url)
        //{
        //    var html = await _httpClient.GetStringAsync(url);

        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(html);


        //    var title =
        //        doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAtributeValue("conent", null)
        //        ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText
        //        ?? doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;

        //    var description =
        //        doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAtributeValue("content", null)
        //        ?? doc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAtributeValue("content", null)
        //        ?? doc.DocumentNode.SelectSingleNode("//p")?.InnerText;

        //    var image =
        //        doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAtributeValue("content", null);

        //    var genreNode = 
        //        doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'genre')]")
        //        ?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'genre')]");

        //    var releaseYear =
        //        doc.DocumentNode.SelectSingleNode("//meta[@property = 'og.year']")?.GetAtributeValue("content", null)
        //        ?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'year')]")?.GetAtributeValue("content", null);

        //    return new BaseResponseWithDataDto<MovieParsedData>
        //    {
        //        IsSuccess = true,
        //        ErrorType = ErrorType.None,
        //        ResponseMessage = "",
        //        Data = new MovieParsedData
        //        {
        //            Title = title?.Trim(),
        //            Description = description?.Trim(),
        //            Image = image,
        //            ReleaseYear = releaseYear
        //        }
        //    };
        //}
    }
}
