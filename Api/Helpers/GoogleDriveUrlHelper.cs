using System.Text.RegularExpressions;

namespace Api.Helpers;

public static class GoogleDriveUrlHelper
{
    public static string? CleanUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        url = url.Trim();

        string fileId = string.Empty;

        var fileDMatch = Regex.Match(url, @"/file/d/([a-zA-Z0-9_-]+)");
        var lh3Match = Regex.Match(url, @"lh3\.googleusercontent\.com/d/([a-zA-Z0-9_-]+)");
        var ucMatch = Regex.Match(url, @"drive\.google\.com/uc\?.*id=([a-zA-Z0-9_-]+)");
        var openIdMatch = Regex.Match(url, @"[?&]id=([a-zA-Z0-9_-]+)");

        if (fileDMatch.Success)
        {
            fileId = fileDMatch.Groups[1].Value;
        }
        else if (lh3Match.Success)
        {
            fileId = lh3Match.Groups[1].Value;
        }
        else if (ucMatch.Success)
        {
            fileId = ucMatch.Groups[1].Value;
        }
        else if (url.Contains("drive.google.com") && openIdMatch.Success)
        {
            fileId = openIdMatch.Groups[1].Value;
        }

        if (!string.IsNullOrEmpty(fileId))
        {
            return $"https://drive.google.com/thumbnail?sz=w1000&id={fileId}";
        }

        return url;
    }
}
