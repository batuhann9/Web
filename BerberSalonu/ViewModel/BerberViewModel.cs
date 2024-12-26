using System.Text.Json.Serialization;

public class BerberViewModel
{
    public int Id { get; set; }

    [JsonPropertyName("adSoyad")]
    public string AdSoyad { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}
