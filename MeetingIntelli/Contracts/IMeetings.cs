using MeetingIntelli.DTO.Requests;

namespace MeetingIntelli.Contracts;

public interface IMeetings
{
    Task<IResult> GetAllMeetings(CancellationToken cancellationToken);
    Task<IResult> GetMeetingById(Guid id, CancellationToken cancellationToken);
    Task<IResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken);
    Task<IResult> UpdateMeeting(Guid id, UpdateMeetingRequest request, CancellationToken cancellationToken);
    Task<IResult> DeleteMeeting(Guid id, CancellationToken cancellationToken);
    Task<IResult> GetStatistics(CancellationToken cancellationToken);
    Task<IResult> GenerateListPdf(IPdfService pdfService, int? viewportWidth, int? viewportHeight, CancellationToken cancellationToken);
    Task<IResult> GenerateMeetingPdf(Guid id, IPdfService pdfService, int? viewportWidth, int? viewportHeight, CancellationToken cancellationToken);
}