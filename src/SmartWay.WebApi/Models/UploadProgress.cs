namespace SmartWay.WebApi.Models;

public class UploadProgress
{
    public Guid? FileId { get; set; }
    public Guid? GroupId { get; set; }
    public int FileUploadProgress { get; set; }
    public int GroupUploadProgress { get; set; }
}