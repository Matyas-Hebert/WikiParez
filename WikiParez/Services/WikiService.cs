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
        _path = Path.Combine(env.ContentRootPath, "appdata", "data.json");
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
                foreach (var metadata in dictionary[key].Metadata)
                {
                    string slug = GetSlugByValue(metadata.Value);
                    if (IsSubdivision(metadata.Label) && dictionary.Keys.Contains(slug))
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
            if (IsSubdivision(dictionary[key].Type))
            {
                dictionary[key].Metadata.Add(new Metadata
                {
                    Label = "Plocha",
                    Value = dictionary[key].area.ToString() + " blok²"
                });
                dictionary[key].Metadata.Add(new Metadata
                {
                    Label = "Počet místností",
                    Value = dictionary[key].numberOfRooms.ToString()
                });
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
            foreach (var metadata in page.Metadata)
            {
                metadata.Value = UseRegex(metadata.Value);
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