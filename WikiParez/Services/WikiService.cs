namespace WikiParez.Services;

using WikiParez.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;

public class WikiService
{
    private readonly string _path;
    private Dictionary<string, WikiPage> _pages;

    public WikiService(IHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "appdata", "pagesData.json");
        _pages = LoadPages();
    }

    public Dictionary<string, SimplifiedWikiPage> GetSimplifiedDict()
    {
        var dict = new Dictionary<string, SimplifiedWikiPage>();
        foreach (var key in _pages.Keys)
        {
            var page = _pages[key];
            if (page.redirect == "" || page.redirect == null)
            {
                dict[key] = new SimplifiedWikiPage
                {
                    Title = page.Title,
                    Type = page.Type,
                    Blok = page.Metadata.ContainsKey("Blok") ? UseRegex(page.Metadata["Blok"]) : "-",
                    Okrsek = page.Metadata.ContainsKey("Okrsek") ? UseRegex(page.Metadata["Okrsek"]) : "-",
                    Ctvrt = page.Metadata.ContainsKey("Čtvrť") ? UseRegex(page.Metadata["Čtvrť"]) : "-",
                    Cast = page.Metadata.ContainsKey("Část") ? UseRegex(page.Metadata["Část"]) : "-",
                    area = page.area,
                    Empty = page.Empty,
                    numberOfRooms = page.numberOfRooms,
                    image_count = page.image_count()
                };
            }
        }
        return dict;
    }

    public string FindBestMatch(string searchValue, int n)
    {
        var best = -1.0;
        string bestString = "";
        foreach (var key in _pages.Keys)
        {
            string name = _pages[key].Title;
            double result = SearchService.GetSimilarityValue(name, searchValue);
            if (result > best)
            {
                best = result;
                bestString = key;
            }

            foreach (var alt in _pages[key].Alternate_names)
            {
                result = SearchService.GetSimilarityValue(alt, searchValue);
                if (result > best)
                {
                    best = result;
                    bestString = key;
                }
            }
        }
        return bestString;
    }

    private string GetSlugByValue(string value)
    {
        return Regex.Replace(value, @"\[(.*?)\]\((.*?)\)", match =>
                {
                    string slug = match.Groups[2].Value;
                    return slug;
                });
    }

    public bool DoesSlugExist(string slug){
        return _pages.Keys.Contains(slug);
    }

    public Dictionary<string, string> Last10pages(){
        int pageCount = _pages.Count;
        var dict = new Dictionary<string, string>();
        for(int i=pageCount-1; i>=pageCount-10; i--){
            var key = _pages.Keys.ElementAt(i);
            if (key.StartsWith("mm") || _pages[key].Empty)
            {
                pageCount--;
            }
            else if (_pages[key].redirect != null && _pages[key].redirect != ""){
                pageCount--;
            }
            else
            {
                dict[key] = _pages[key].Title;
            }
        }
        return dict;
    }

    public string? GetRandomSlug(bool canBeEmpty)
    {
        if (_pages == null || _pages.Count == 0)
            return null;

        var keys = new List<string>(_pages.Keys);
        var random = new Random();
        var randomKey = keys[random.Next(keys.Count)];
        while ((_pages[randomKey].Empty && !canBeEmpty) || (_pages[randomKey].redirect != "" && _pages[randomKey].redirect != null))
        {
            randomKey = keys[random.Next(keys.Count)];
        }
        return randomKey;
    }

    public string? GetRandomRoomSlug(bool canBeEmpty){
        if (_pages == null || _pages.Count == 0)
            return null;

        var keys = new List<string>(_pages.Keys);
        var random = new Random();
        var randomKey = keys[random.Next(keys.Count)];
        while (!randomKey.StartsWith("mi") || (_pages[randomKey].Empty && !canBeEmpty) || (_pages[randomKey].redirect != "" && _pages[randomKey].redirect != null)){
            randomKey = keys[random.Next(keys.Count)];
        }
        return randomKey;
    }

    private bool IsSubdivision(string type)
    {
        return type != null &&
            (type.Equals("blok", StringComparison.OrdinalIgnoreCase) ||
             type.Equals("okrsek", StringComparison.OrdinalIgnoreCase) ||
             type.Equals("čtvrť", StringComparison.OrdinalIgnoreCase) ||
             type.Equals("část", StringComparison.OrdinalIgnoreCase) ||
             type.Equals("multi", StringComparison.OrdinalIgnoreCase) ||
             type.Equals("kategorie", StringComparison.OrdinalIgnoreCase));
    }

    void PrintSortedDictionary(Dictionary<string, int> dict, List<string> keys)
    {
        double total = 0;
        double done = 0;
        foreach (var pair in dict.OrderByDescending(kvp => kvp.Value))
        {
            if (!keys.Contains(pair.Key))
            {
                done += pair.Value;
                Console.ForegroundColor = ConsoleColor.Red;
            }
            total += pair.Value;
            Console.WriteLine($"{pair.Key}: {pair.Value}");
            Console.ResetColor();
        }
        done = total - done;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(done + "/" + total + " (" + done/total + ")");
        Console.ResetColor();
    }

    public void Analyze(Dictionary<string, WikiPage> dictionary){
        var linksDict = new Dictionary<string, int>();
        var lengths = new Dictionary<string, int>();
        int total = 0;
        int articles = 0;
        foreach (var key in dictionary.Keys)
        {
            if (!key.StartsWith("mm"))
            {
                articles++;
                foreach (var section in dictionary[key].Sections)
                {
                    string result = Regex.Replace(section.Content, @"\[(.*?)\]\((.*?)\)", match =>
                    {
                        string name = match.Groups[1].Value;
                        string link = match.Groups[2].Value;

                        if (!linksDict.ContainsKey(link))
                            linksDict[link] = 0;
                        linksDict[link]++;

                        return $"{name}";
                    });
                    if (!lengths.ContainsKey(key))
                    {
                        lengths[key] = 0;
                    }
                    lengths[key] += result.Length;
                    total += result.Length;
                }
                foreach (var mkey in dictionary[key].Metadata.Keys)
                {
                    if (dictionary[key].Metadata[mkey] != null)
                    {
                        string result = Regex.Replace(dictionary[key].Metadata[mkey], @"\[(.*?)\]\((.*?)\)", match =>
                        {
                            string name = match.Groups[1].Value;
                            string link = match.Groups[2].Value;

                            if (!linksDict.ContainsKey(link))
                                linksDict[link] = 0;
                            linksDict[link]++;

                            return $"{name}";
                        });
                    }
                }
            }
        }

        List<string> dictKeys = dictionary.Keys.ToList();

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("lengths");
        Console.ResetColor();
        PrintSortedDictionary(lengths, dictKeys);
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("links");
        Console.ResetColor();
        PrintSortedDictionary(linksDict, dictKeys);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Total of: " + total + " characters in " + articles + " articles.");
        Console.ResetColor();
    }

    public Dictionary<string, WikiPage> LoadPages()
    {
        if (!File.Exists(_path))
            return new Dictionary<string, WikiPage>();

        var json = File.ReadAllText(_path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, WikiPage>>(json, options) ?? new Dictionary<string, WikiPage>();
        Analyze(dictionary);
        foreach (var key in dictionary.Keys)
        {
            if (dictionary[key].image_count() == 0)
            {
                dictionary[key].Images.Add("missing.jpg");
                dictionary[key].Empty = true;
                dictionary[key].Image_titles.Add("");
            }
            else if (!IsSubdivision(dictionary[key].Type))
            {
                for (int i = 1; i <= dictionary[key].image_count(); i++)
                {
                    dictionary[key].Images.Add($"{key}{i}.png");
                }
            }
            if (!IsSubdivision(dictionary[key].Type))
            {
                if (dictionary[key].redirect != null && dictionary[key].redirect != "")
                {
                    var red = dictionary[key].redirect;
                    for (int i = 0; i < dictionary[key].image_count(); i++)
                    {
                        dictionary[red].Images.Add(dictionary[key].Images[i]);
                    }
                }
                foreach (var dataKey in dictionary[key].Metadata.Keys)
                {
                    string slug = GetSlugByValue(dictionary[key].Metadata[dataKey]);
                    //Console.WriteLine("metadata key" + dataKey + " page key: " + key + "value: " + slug);
                    if (IsSubdivision(dataKey) && dictionary.Keys.Contains(slug))
                    {
                        dictionary[slug].area += dictionary[key].area;
                        if (dictionary[key].Type == "místnost") dictionary[slug].numberOfRooms++;
                        if (!dictionary[key].Empty)
                        {
                            for (int i = 0; i < dictionary[key].image_count(); i++)
                            {
                                dictionary[slug].Images.Add(dictionary[key].Images[i]);
                                dictionary[slug].Image_titles.Add("<a href=\"/" + key + "\">" + dictionary[key].Image_titles[i] + "</a>");
                            }
                        }
                    }
                }
            }
        }
        return dictionary;
    }

    private string UseRegex(string content)
    {
        return Regex.Replace(content, @"\[(.*?)\]\((.*?)\)", match =>
                {
                    string label = match.Groups[1].Value;
                    string slug = match.Groups[2].Value;
                    return $"<a href=\"/{slug}\">{label}</a>";
                });
    }

    private void AddLinks(WikiPage page)
    {
        if (page?.Sections != null)
        {
            foreach (var section in page.Sections)
            {
                section.Content = UseRegex(section.Content);
            }
        }
        if (page?.Metadata != null)
        {
            foreach (var key in page.Metadata.Keys)
            {
                page.Metadata[key] = UseRegex(page.Metadata[key]);
            }
        }
    }

    public WikiPage? GetPageBySlug(string slug)
    {
        if (_pages != null && _pages.TryGetValue(slug, out var page))
        {
            AddLinks(page);
            return page;
        }
        // Implementation to retrieve the WikiPage by slug
        return null;
    }
}