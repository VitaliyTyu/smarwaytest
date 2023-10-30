namespace SmartWay.WebApi.Models;

public class UploadProgress
{
    public Guid? LastFileId { get; set; }
    public Guid? GroupId { get; set; }
    public int LastFileUploadProgress { get; set; }
    public int GroupUploadProgress { get; set; }
}