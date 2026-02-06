namespace MeetingIntelli.Services
    .Interface;

public interface IPdfService
{


    Task<byte[]> GeneratePdfFromUrlAsync(string urlPath, CancellationToken ct = default);
}
