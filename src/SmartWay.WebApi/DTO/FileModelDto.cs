namespace SmartWay.WebApi.DTO;

public class FileModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public Guid GroupId { get; set; }
}