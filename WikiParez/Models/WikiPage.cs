namespace WikiParez.Models;

public class WikiPage
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    public List<string> Image_titles { get; set; } = new List<string>();
    public List<string> Bordering_rooms { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public List<Section> Sections { get; set; } = new List<Section>();
    public int image_id { get; set; } = 0;
    public int area { get; set; } = 0;
    public bool Empty {get; set; } = false;
    public int numberOfRooms { get; set; } = 0;
    public int image_count()
    {
        return Image_titles.Count;
    }
}

public class Section
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}