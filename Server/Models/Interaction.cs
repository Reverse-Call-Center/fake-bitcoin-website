namespace Server.Models;

public record Interaction
{
    public required string Type { get; set; }
    public required string Data { get; set; }
}