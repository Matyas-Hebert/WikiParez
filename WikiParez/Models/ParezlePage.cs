namespace WikiParez.Models;

public class ParezlePage
{
    public string Title { get; set; } = string.Empty;
    public List<string> Bordering_rooms { get; set; } = new List<string>();
    public string Blok { get; set; } = string.Empty;
    public string Okrsek { get; set; } = string.Empty;
    public string Ctvrt { get; set; } = string.Empty;
    public string Cast { get; set; } = string.Empty;
}