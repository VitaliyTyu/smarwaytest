namespace SmartWay.WebApi.Models;

public class FileModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public Guid GroupId { get; set; }
}