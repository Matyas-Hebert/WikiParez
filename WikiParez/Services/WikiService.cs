namespace WikiParez.Services;

using WikiParez.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        return JsonSerializer.Deserialize<Dictionary<string, WikiPage>>(json, options) ?? new Dictionary<string, WikiPage>();
    }

    public WikiPage GetPageBySlug(string slug)
    {
        if (_pages != null && _pages.TryGetValue(slug, out var page))
        {
            return page;
        }
        // Implementation to retrieve the WikiPage by slug
        return null;
    }
}