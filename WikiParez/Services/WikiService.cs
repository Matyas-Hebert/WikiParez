namespace WikiParez.Services;

using WikiParez.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;

public class WikiService
{
    private readonly string _path;
    private readonly string _pathsplash;
    private Dictionary<string, WikiPage> _pages;
    private Dictionary<string, WikiPage> _onlyroomspages;
    private Dictionary<string, SimplifiedWikiPage> _simplifiedPages;
    private Dictionary<string, Coordinates> _patternlepages;

    private int finishedPages = 0;
    private int totalpages = 0;

    public WikiService(IHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "appdata", "pagesData.json");
        _pathsplash = Path.Combine(env.ContentRootPath, "appdata", "splashtexts.txt");
        _pages = LoadPages();
        _onlyroomspages = LoadOnlyRoomPages();
        _simplifiedPages = GetSimplifiedDict();
        _patternlepages = LoadCoordinatesPages();
    }

    public Dictionary<string, ParezlePage> GetParezlePages()
    {
        var pages = new Dictionary<string, ParezlePage>();
        foreach (var key in _onlyroomspages.Keys)
        {
            pages.Add(key, new ParezlePage
            {
                Title = _onlyroomspages[key].Title,
                Bordering_rooms = _onlyroomspages[key].Bordering_rooms,
                Blok = GetNameFromLink(_onlyroomspages[key].Metadata["Blok"]),
                Okrsek = GetNameFromLink(_onlyroomspages[key].Metadata["Okrsek"]),
                Ctvrt = GetNameFromLink(_onlyroomspages[key].Metadata["Čtvrť"]),
                Cast = GetNameFromLink(_onlyroomspages[key].Metadata["Část"])
            });
        }
        return pages;
    }

    private Dictionary<string, Coordinates> LoadCoordinatesPages()
    {
        var coords = new Dictionary<string, Coordinates>();
        var i = 0;
        foreach (var page in _pages.Keys)
        {
            if (_pages[page].coordinates.x != 0 && _pages[page].coordinates.y != 0 && _pages[page].coordinates.z != 0)
            {
                Console.WriteLine(page);
                i++;
                coords[page] = _pages[page].coordinates;
            }
        }
        Console.WriteLine(i + "rooms");
        return coords;
    }

    public Dictionary<string, Coordinates> GetNCoordinates(int n)
    {
        var availableKeys = _patternlepages.Keys.ToList();
        var dict = new Dictionary<string, Coordinates>();
        for (int i = 0; i < n; i++)
        {
            var random = new Random();
            var randomkey = availableKeys[random.Next(availableKeys.Count)];
            dict[_pages[randomkey].Title] = _patternlepages[randomkey];
            availableKeys.Remove(randomkey);
        }
        return dict;
    }

    private string GetNameFromLink(string link)
    {
        var match = Regex.Match(link, @"\[(.*?)\]\((.*?)\)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        match = Regex.Match(link, @"<a[^>]*>(.*?)</a>");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return link;
    }

    public Dictionary<string, WikiPage> LoadOnlyRoomPages()
    {
        Dictionary<string, WikiPage> onlyroompages = new Dictionary<string, WikiPage>();
        foreach (var key in _pages.Keys)
        {
            if (_pages[key].Type == "místnost" && key.StartsWith("mi"))
            {
                onlyroompages.Add(key, _pages[key]);
            }
        }
        return onlyroompages;
    }

    public Quaternion RandomQuaternion()
    {
        double x, y, z, u, v, w, s;
        var random = new Random();
        do
        {
            x = random.NextDouble() * 2 - 1;
            y = random.NextDouble() * 2 - 1;
            z = Math.Pow(x, 2) + Math.Pow(y, 2);
        } while (z > 1);
        do
        {
            u = random.NextDouble() * 2 - 1;
            v = random.NextDouble() * 2 - 1;
            w = Math.Pow(u, 2) + Math.Pow(v, 2);
        } while (w > 1);
        s = Math.Sqrt((1 - z) / w);
        return new Quaternion((float)x, (float)y, (float)(s * u), (float)(s * w));
    }

    public int GetParezleSeed()
    {
        var utcNow = DateTime.UtcNow;
        var pragueTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
        var pragueTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pragueTimeZone);
        var seed = pragueTime.Year * 366 + pragueTime.DayOfYear;
        return seed;
    }

    private List<string> GetBordering(string slug)
    {
        if (_pages == null || !_pages.ContainsKey(slug))
            return new List<string>();

        var page = _pages[slug];
        return page.Bordering_rooms;
    }

    public bool DoesBorder(string room1, string room2)
    {
        if (_pages[room1].Bordering_rooms.Contains(room2)) return true;
        return false;
    }

    public List<string> FindPath(string from, string to)
    {
        if (from == "mi_listy" || to == "mi_listy" || to == "mi_svatyne")
        {
            return new List<string>();
        }

        Queue<string> toexplore = new Queue<string>();
        HashSet<string> visited = new HashSet<string>();
        Dictionary<string, string?> previous = new Dictionary<string, string?>();

        previous.Add(from, null);
        toexplore.Enqueue(from);

        while (toexplore.Count > 0)
        {
            string room = toexplore.Dequeue();
            if (!visited.Contains(room))
            {
                if (room == to)
                {
                    List<string> path = new List<string>();
                    path.Add(to);
                    var backtrack = room;
                    while (previous[backtrack] != null)
                    {
                        backtrack = previous[backtrack];
                        path.Add(backtrack);
                    }
                    return path;
                }
                var bordering = GetBordering(room);
                foreach (var borderingroom in bordering)
                {
                    if (!previous.ContainsKey(borderingroom))
                    {
                        toexplore.Enqueue(borderingroom);
                        previous.Add(borderingroom, room);
                    }
                }
                visited.Add(room);
            }
        }

        return new List<string>();
    }

    public Dictionary<string, SimplifiedWikiPage> GetSimplifiedDict()
    {
        if (_simplifiedPages != null)
        {
            return _simplifiedPages;
        }
        var dict = new Dictionary<string, SimplifiedWikiPage>();
        foreach (var key in _pages.Keys)
        {
            var page = _pages[key];
            if ((page.redirect == "" || page.redirect == null) && page.Type != "secret")
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
            if (_pages[key].Type != "secret")
            {
                string name = _pages[key].Title;
                double result = SearchService.GetSimilarityValue(name, searchValue);
                if (result > best)
                {
                    best = result;
                    bestString = key;
                }

                string subdivname = _pages[key].Type + _pages[key].Title;
                string namesubdiv = _pages[key].Title + _pages[key].Type;

                result = SearchService.GetSimilarityValue(subdivname, searchValue);
                if (result > best)
                {
                    best = result;
                    bestString = key;
                }
                result = SearchService.GetSimilarityValue(subdivname, searchValue);
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
        Console.WriteLine("list:");
        for (int i = pageCount - 1; i >= pageCount - 10; i--)
        {
            var key = _pages.Keys.ElementAt(i);
            if (_pages[key].Empty || _pages[key].Type == "secret")
            {
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
        while ((_pages[randomKey].Empty && !canBeEmpty) || (_pages[randomKey].redirect != "" && _pages[randomKey].redirect != null) || _pages[randomKey].Type == "secret")
        {
            randomKey = keys[random.Next(keys.Count)];
        }
        return randomKey;
    }

    public string GetRandomRoomSlug(bool canBeEmpty){
        var keys = new List<string>(_onlyroomspages.Keys);
        var random = new Random();
        var randomKey = keys[random.Next(keys.Count)];
        while (!randomKey.StartsWith("mi") || (_onlyroomspages[randomKey].Empty && !canBeEmpty) || (_onlyroomspages[randomKey].redirect != "" && _onlyroomspages[randomKey].redirect != null)){
            randomKey = keys[random.Next(keys.Count)];
        }
        return randomKey;
    }

    public int GetNumberOfRooms()
    {
        return _onlyroomspages.Count;
    }

    public string GetRoomSlugByID(int id)
    {
        return _onlyroomspages.Keys.ElementAt(id);
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
            if (pair.Value != 0) Console.WriteLine($"{pair.Key}: {pair.Value}");
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
        int area = 0;
        foreach (var key in dictionary.Keys)
        {
            if (!key.StartsWith("mm"))
            {
                area += dictionary[key].area;
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
                    if (dictionary[key].Empty == false && dictionary[key].Type != "redirect")
                    {
                        lengths[key] += result.Length;
                        total += result.Length;
                    }
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
        Console.WriteLine("area:" + area);

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

    public Dictionary<string, WikiPage> GetPages()
    {
        return _pages;
    }

    public List<string> getSplashTexts()
    {
        return File.ReadLines(_pathsplash).ToList();
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
            totalpages++;
            if (dictionary[key].Empty == false && dictionary[key].Type != "redirect") finishedPages++;
            if (dictionary[key].image_count() == 0)
            {
                dictionary[key].Images.Add("missing.jpg");
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

    public int getFinishedPages()
    {
        return finishedPages;
    }

    public int getTotalPages()
    {
        return totalpages;
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

    private WikiPage? AddLinks(WikiPage page)
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
        return page;
    }

    public WikiPage? GetPageBySlug(string slug)
    {
        if (_pages != null && _pages.TryGetValue(slug, out var page))
        {
            page = AddLinks(page);
            return page;
        }
        // Implementation to retrieve the WikiPage by slug
        return null;
    }
}