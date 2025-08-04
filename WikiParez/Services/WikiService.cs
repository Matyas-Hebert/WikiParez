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

    public Dictionary<string, double> GetTopParezleRooms()
    {
        return new Dictionary<string, double>
        {
            {"The Bridge",33778.12},{"Dálniční Pilíř",26571.87},{"Dálnice",25446.82},{"Venkovní Schodiště",19087.51},{"Kořeny",16244.06},{"Zoban",15462},{"Ptakopysk",14646.52},{"Liána",14622.86},{"Silo",14137.13},{"Výtahová Šachta",13368.44},{"Veranda",12706.92},{"Dvojčata",12449.64},{"Okruh",11930.51},{"Hráz",10656.42},{"Kanál",10638.93},{"Hlavní Koridor II",9601.96},{"Středové náměstí",9387.03},{"Kahan",8662.14},{"The Airship",8574.96},{"Smyčka",8499.77},{"Papokoi",8218.94},{"Karotka",8019.35},{"The Detour",7816.39},{"Lilek",7482.14},{"Pavlač",7478.95},{"The Overpass",7462.49},{"The Crossing",7214.47},{"Preatrium",6970.76},{"Kokarda",6699.71},{"Labyrint",6354.53},{"Únikové Schodiště",5692.35},{"Rozhraní",5250.28},{"Alej",5059.34},{"Přístavní Molo",4951.3},{"Silice",4924.72},{"Díra do Pekel",4754.8},{"Farmus-Bambus",4484.93},{"Spojka",4442.7},{"Atrium",4372.75},{"Hotel Gi Floor Ga",4190.54},{"Hlavní Koridor I",4182.08},{"Pod Schodama",3928.96},{"Královské Abonmá",3864.44},{"Vstup",3816.96},{"Jižní Blok",3798.02},{"Náměstí u Vody",3768.82},{"Obalovna",3698.61},{"Katakomby",3680.76},{"Železná Lhota",3554.04},{"Klub",3382.45},{"Pata",3367.03},{"Čtverec",3288.58},{"Pod Vrbou",3245.23},{"Statek",3191.52},{"Diagonála",3167.91},{"Závodní Okruh",3125.44},{"Zmatek",3092.85},{"Sluníčko",3030.88},{"Trója",3024.64},{"Hlavní Třída",3018.36},{"Dragon Lair",3005.96},{"Stoupání",2955.96},{"Juka",2943.81},{"Lom",2906.02},{"Výplň",2881.03},{"Hotel Gi Floor Gi",2767.37},{"Městský Okruh",2610.45},{"Amazonský Havířov",2602.61},{"Hvězda",2528.7},{"Loosova Vila",2492.2},{"Guacamole",2487.07},{"Hnízdo",2470.06},{"Garáž",2450.79},{"Chodba",2444.11},{"Gideon",2391.66},{"Východní Blok",2391.54},{"Spirála",2381.39},{"UFO",2368.4},{"Zlaté Schodiště",2367.94},{"Spojnice",2283.31},{"Ortofrater",2214.54},{"Preddvor",2108.44},{"Na Můstku",2106.03},{"Na Ochozu",2098.55},{"Balónky",2091.91},{"Laboratoř",2049.66},{"Le Pont",2003.45},{"Ústřední středisko Alfa",1992.75},{"Dračí Rotunda",1961.52},{"Hotel Gi Floor Ge",1904.77},{"Pasáž",1817.55},{"Dron",1794.12},{"Ústřední středisko Beta",1771.27},{"Zahrádkářská Kolonie",1748.41},{"Arkáda",1711.63},{"Hotel",1702.88},{"Méďa",1643.81},{"Dolní Nádraží",1636.73},{"Přístav",1633.75},{"Tělocvična",1624.87}
        };
    }

    public WikiService(IHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "appdata", "pagesData.json");
        _pathsplash = Path.Combine(env.ContentRootPath, "appdata", "splashtexts.txt");
        Console.WriteLine(_path);
        Console.WriteLine(_pathsplash);
        _pages = LoadPages();
        _onlyroomspages = LoadOnlyRoomPages();
        _simplifiedPages = GetSimplifiedDict();
        _patternlepages = LoadCoordinatesPages();
        var max = 0;
        var scores = new Dictionary<string, double>();
        foreach (var room1 in _onlyroomspages.Keys)
        {
            foreach (var room2 in _onlyroomspages.Keys)
            {
                if (!DoesBorder(room1, room2) && room1 != room2)
                {
                    var paths = FindPaths(room1, room2);
                    foreach (var path in paths)
                    {
                        if (path.Count >= max)
                        {
                            max = path.Count;
                            Console.WriteLine("found path with length: " + max + " " + room1 + " to " + room2);
                        }
                        var len = path.Count;
                        foreach (var room in path)
                        {
                            if (!scores.ContainsKey(room))
                            {
                                scores[room] = 0;
                            }
                            if (room != room1 && room != room2)
                            {
                                scores[room] += 1.0f / len;
                            }
                        }
                    }
                }
            }
        }

        scores = scores.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        foreach (var scorekey in scores.Keys)
        {
            Console.WriteLine("{\""+ _pages[scorekey].Title + "\"," + Math.Round(scores[scorekey], 2) + "},");
        }
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
            if (_pages[page].coordinates.x != 0 && _pages[page].coordinates.y != 0 && _pages[page].coordinates.z != 0 && _pages[page].patternleCompatible)
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

    public List<List<string>> FindPaths(string from, string to)
    {
        //Console.WriteLine("finding paths from " + from + " to " + to);
        var toexplore = new Queue<string>();
        var distance = new Dictionary<string, int>();
        var predecessors = new Dictionary<string, List<string>>();

        predecessors[from] = new List<string>();
        distance[from] = 0;
        toexplore.Enqueue(from);

        while (toexplore.Count > 0)
        {
            var currentroom = toexplore.Dequeue();
            var currentdistance = distance[currentroom];
            var borderingrooms = GetBordering(currentroom);
            foreach (var borderingroom in borderingrooms)
            {
                if (!distance.ContainsKey(borderingroom))
                {
                    distance[borderingroom] = currentdistance+1;
                    predecessors[borderingroom] = new List<string> { currentroom };
                    toexplore.Enqueue(borderingroom);
                }
                else if (distance[borderingroom] == currentdistance + 1)
                {
                    predecessors[borderingroom].Add(currentroom);
                }
            }
        }

        var allpaths = new List<List<string>>();

        if (!predecessors.ContainsKey(to)) return allpaths;

        void Backtrack(string current, List<string> path)
        {
            if (current == from)
            {
                var completePath = new List<string>(path) { from };
                completePath.Reverse();
                allpaths.Add(completePath);
                return;
            }

            foreach (var prev in predecessors[current])
            {
                path.Add(current);
                Backtrack(prev, path);
                path.RemoveAt(path.Count - 1);
            }
        }

        Backtrack(to, new List<string>());

        return allpaths;
    }

    public List<string> FindPath(string from, string to)
    {
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
            
            if (dictionary[key].Type != "redirect") totalpages++;
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