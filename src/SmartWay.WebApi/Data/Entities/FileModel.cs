namespace SmartWay.WebApi.Data.Entities;

public class FileModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string UserId { get; set; }
    public Guid GroupId { get; set; }
}