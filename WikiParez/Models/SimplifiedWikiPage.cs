namespace WikiParez.Models;

public class SimplifiedWikiPage
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    //public List<string> Bordering_rooms { get; set; } = new List<string>();
    //public List<string> Alternate_names { get; set; } = new List<string>();
    public string Blok { get; set; } = string.Empty;
    public string Okrsek { get; set; } = string.Empty;
    public string Ctvrt { get; set; } = string.Empty;
    public string Cast { get; set; } = string.Empty;
    public int area { get; set; } = 0;
    public bool Empty { get; set; } = false;
    public int numberOfRooms { get; set; } = 0;
    public int image_count { get; set; } = 0;
}