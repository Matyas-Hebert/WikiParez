namespace WikiParez.Models;

public class WikiPage
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    public List<string> Image_titles { get; set; } = new List<string>();
    public List<Metadata> Metadata { get; set; } = new List<Metadata>();
    public List<Section> Sections { get; set; } = new List<Section>();
    public int image_id { get; set; } = 0;
    public int image_count()
    {
        return Images.Count;
    }
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