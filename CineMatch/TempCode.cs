//using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using System.Text.Json;

//List<JsonElement?> results = new();
//foreach (var bestMatchs in resultsArray.EnumerateArray())
//{
//    string? title = null;
//    if (inputType == ContentType.movie && bestMatchs.TryGetProperty("title", out var titleProp))
//        title = titleProp.GetString();
//    else if (inputType == ContentType.tv && bestMatchs.TryGetProperty("name", out var nameProp))
//        title = nameProp.GetString();

//    if (string.Equals(title, inputTitle, StringComparison.OrdinalIgnoreCase))
//    {
//        results = bestMatchs;
//        break;
//    }
//}
//if (firstResult == null)
//{
//    firstResult = resultsArray.EnumerateArray().FirstOrDefault();
//}
//if (firstResult == null)
//{
//    return null;
//}
