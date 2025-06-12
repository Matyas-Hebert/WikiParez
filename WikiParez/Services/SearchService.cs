namespace WikiParez.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public class SearchService
{
    public static double GetSimilarityValue(string a, string b)
    {
        a = Normalize(a);
        b = Normalize(b);

        if (a.Length < 2 || b.Length < 2) return a == b ? 1.0 : 0.0;

        var aCombinations = GetCombinations(a);
        var bCombinations = GetCombinations(b);

        var intersect = aCombinations.Intersect(bCombinations).Count();

        return (2.0 * intersect) / (aCombinations.Count + bCombinations.Count);
    }

    private static string Normalize(string input)
    {
        input = input.ToLowerInvariant();
        input = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in input)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Replace(" ", "");
    }

    private static List<string> GetCombinations(string input)
    {
        var combinations = new List<string>();
        for (int i = 0; i < input.Length - 1; i++)
        {
            combinations.Add(input.Substring(i, 2));
        }
        return combinations;
    }
}