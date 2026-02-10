namespace MeetingIntelli.Interface;

public interface IPdfService
{

    Task<byte[]> GeneratePdfFromUrlAsync(
       string urlPath,
       int? viewportWidth = null,
       int? viewportHeight = null,
       CancellationToken cancellationToken = default);
   

}
