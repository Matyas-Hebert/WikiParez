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
    public Coordinates coordinates { get; set; } = new Coordinates();
    public bool patternleCompatible { get; set; } = true;
    public int image_count()
    {
        return Image_titles.Count;
    }
    public string redirect { get; set; } = string.Empty;
    public List<Review> reviews { get; set; } = new List<Review>();
    public int review_count()
    {
        return reviews.Count;
    }

    public HashSet<string> utilities { get; set; } = new HashSet<string>();

    public WikiPage Clone()
    {
        WikiPage clone = (WikiPage)this.MemberwiseClone();

        // Step 2: manually deep-copy mutable fields
        clone.Images = new List<string>(this.Images);
        clone.Image_titles = new List<string>(this.Image_titles);
        clone.Bordering_rooms = new List<string>(this.Bordering_rooms);
        clone.Alternate_names = new List<string>(this.Alternate_names);
        clone.Metadata = new Dictionary<string, string>(this.Metadata);
        clone.Sections = this.Sections.Select(s => new Section
        {
            Title = s.Title,
            Content = s.Content
        }).ToList();
        clone.reviews = this.reviews.Select(r => new Review
        {
            author = r.author,
            stars = r.stars,
            text = r.text
        }).ToList();

        // Coordinates is a class, so deep-copy it too if needed
        clone.coordinates = new Coordinates
        {
            x = this.coordinates.x,
            y = this.coordinates.y,
            z = this.coordinates.z
        };
        clone.utilities = new HashSet<string>(this.utilities);

        return clone;
    }
}

public class Review
{
    public double stars { get; set; } = 0;
    public string author { get; set; } = string.Empty;
    public string text { get; set; } = string.Empty;
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