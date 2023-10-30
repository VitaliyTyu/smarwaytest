namespace SmartWay.WebApi.Data.Entities;

public class DownloadLink
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid? FileId { get; set; }
    public Guid? GroupId { get; set; }
    public string Url { get; set; }
}