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
            if (dictionary[key].Images.Count == 0)
            {
                dictionary[key].Images.Add("missing.jpg");
                dictionary[key].Image_titles.Add("");
            }
        }
        return dictionary;
    }

    private string UseRegex(string content) {
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