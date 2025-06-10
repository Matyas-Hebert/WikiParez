namespace WikiParez.Services;

using WikiParez.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;

public class WikiService
{
    private readonly string _path;
    private Dictionary<string, WikiPage> _pages;

    public WikiService(IHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "appdata", "pagesData.json");
        _pages = LoadPages();
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
            dict[key] = _pages[key].Title;
        }
        return dict;
    }

    public string? GetRandomSlug()
    {
        if (_pages == null || _pages.Count == 0)
            return null;

        var keys = new List<string>(_pages.Keys);
        var random = new Random();
        var randomKey = keys[random.Next(keys.Count)];
        return randomKey;
    }

    public string? GetRandomRoomSlug(){
        if (_pages == null || _pages.Count == 0)
            return null;

        var keys = new List<string>(_pages.Keys);
        var random = new Random();
        var randomKey = keys[random.Next(keys.Count)];
        while (!randomKey.StartsWith("mi")){
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
             type.Equals("část", StringComparison.OrdinalIgnoreCase));
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
                foreach (var dataKey in dictionary[key].Metadata.Keys)
                {
                    string slug = GetSlugByValue(dictionary[key].Metadata[dataKey]);
                    if (IsSubdivision(dataKey) && dictionary.Keys.Contains(slug))
                    {
                        dictionary[slug].area += dictionary[key].area;
                        if (dictionary[key].Type == "místnost") dictionary[slug].numberOfRooms++;
                        if (!dictionary[key].Empty)
                        {
                            for(int i = 0; i < dictionary[key].image_count(); i++)
                            {
                                dictionary[slug].Images.Add(dictionary[key].Images[i]);
                                dictionary[slug].Image_titles.Add("<a href=\"/"+key+"\">"+dictionary[key].Image_titles[i]+"</a>");
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