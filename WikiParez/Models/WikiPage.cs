namespace WikiParez.Models;

public class WikiPage
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    public List<string> Image_titles { get; set; } = new List<string>();
    public List<string> Bordering_rooms { get; set; } = new List<string>();
    public List<string> Alternate_names { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public List<Section> Sections { get; set; } = new List<Section>();
    public int image_id { get; set; } = 0;
    public int area { get; set; } = 0;
    public bool Empty { get; set; } = false;
    public int numberOfRooms { get; set; } = 0;
    public Coordinates coordinates {get; set;} = new Coordinates();
    public int image_count()
    {
        return Image_titles.Count;
    }
    public string redirect { get; set; } = string.Empty;
}

public class Coordinates
{
    public int x { get; set; } = 0;
    public int y { get; set; } = 0;
    public int z { get; set; } = 0;
    
    public double distanceFrom(Coordinates a) {
        return Math.Sqrt(Math.Pow(a.x - x, 2) + Math.Pow(a.y - y, 2) + Math.Pow(a.z - z, 2));
    }
}

public class Section
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}