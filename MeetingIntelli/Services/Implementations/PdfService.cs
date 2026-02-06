using MeetingIntelli.Configurations;
using MeetingIntelli.Services.Interface;
using System.Runtime.CompilerServices;

namespace MeetingIntelli.Services.Implementations;

public class PdfService : IPdfService
{
    private readonly FrontEndSettings _frontEndSettings;
    private readonly ILogger<PdfService> _logger;
    public PdfService( FrontEndSettings frontEndSettings, ILogger<PdfService> logger){

        _logger = logger;
        _frontEndSettings = frontEndSettings;

    }
    public Task<byte[]> GeneratePdfFromUrlAsync(string urlPath, CancellationToken ct = default)
    {
        
    }
}
