namespace WikiParez.Models;

public class VlaklePage
{
    public string Title { get; set; } = string.Empty;
    public List<string> Bordering_rooms { get; set; } = new List<string>();
    public HashSet<string> utilities { get; set; } = new HashSet<string>();
}