namespace WikiParez.Models;

public class WikiPage
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<Metadata> Metadata { get; set; } = new List<Metadata>();
    public List<Section> Sections { get; set; } = new List<Section>();
}

public class Metadata
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Section
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}