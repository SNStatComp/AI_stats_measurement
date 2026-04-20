using System.Text.RegularExpressions;

namespace AI_stats_measurement.Backend.Services.Parsing
{
    public static class TextCleaner
    {
        public static string Clean(string text)
        {
            // remove full Dutch dates like "1 januari 2021"
            text = Regex.Replace(
                text,
                @"\b\d{1,2}\s+(januari|februari|maart|april|mei|juni|juli|augustus|september|oktober|november|december)\s+(19|20)\d{2}\b",
                "",
                RegexOptions.IgnoreCase
            );

            text = Regex.Replace(text, @"https?:\/\/\S+", ""); // remove urls
            text = Regex.Replace(text, @"\*\*", "");           // markdown bold
            text = Regex.Replace(text, @"\b(19|20)\d{2}-\w+", "", RegexOptions.IgnoreCase); // remove patterns like "2020/2021"  
            text = Regex.Replace(text, @"^\d{4}\s*/\s*\d{4}$", ""); // remove standalone year ranges like "1990/2000"
            text = Regex.Replace(text, @"(?is)\n\s*\*{0,2}\s*(bron|source)\s*:\s*.*$", ""); // remove eveting after with "Source:" or "Bron:"
            text = Regex.Replace(text, @"\b\d{1,3}\s*[-–]\s*\d{1,3}\s*(jaar|years)?\b", "", RegexOptions.IgnoreCase); // remove patterns like "5-10 jaar"
            text = Regex.Replace(text, @"\b(19|20)\d{2}\s*=\s*\d+\b", "", RegexOptions.IgnoreCase); // remove patterns like "2021=100"                                                                                             // remove citations like [[1]]
            text = Regex.Replace(text, @"\[\[\s*\d+\s*\]\]", ""); // remove citations like [[1]]
            text = Regex.Replace(text, @"\bin\s+(19|20)\d{2}\b", "", RegexOptions.IgnoreCase); // remove years "in 2021"
            text = Regex.Replace(text, @"\b(19|20)\d{2}\b", ""); // remove "2021-cijfers"

            // Remove patterns like "tabel 1A"
            text = Regex.Replace(text, @"\b(tabel|table)\s*\(?\s*\d+[A-Z]+\s*\)?", "", RegexOptions.IgnoreCase);

            text = Regex.Replace(text, @"\(\s*\d+e\s+\w+\s*\)", "", RegexOptions.IgnoreCase); // remove patterns like "(1e kwartaal)"

            text = Regex.Replace(text, @"\b\d+e\b", "", RegexOptions.IgnoreCase); // remove patterns like "1e" when not followed by a month (to avoid removing "1e januari")

            text = Regex.Replace(text, @"\b[A-Z]{1,5}\d+\b", "", RegexOptions.IgnoreCase); // remove patterns like "CBS123"

            text = Regex.Replace(
                text,
                @"\b(aged|age|leeftijd)?\s*\d{1,3}\s*(to|tot)\s*\d{1,3}\s*(jaar|years)?\b", // remove patterns like "age 5 to 10 years" or "leeftijd 5 tot 10 jaar"
                "",
                RegexOptions.IgnoreCase
            );
            return text;
        }
    }
}
