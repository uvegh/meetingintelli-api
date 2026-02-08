using MeetingIntelli.DTO.Requests;
using MeetingIntelli.Interface;

namespace MeetingIntelli.Contracts;

public interface IMeetings
{
    Task<IResult> GetAllMeetings(CancellationToken ct);

    Task<IResult> GetMeetingById(Guid id, CancellationToken ct);

    Task<IResult> CreateMeeting(
        CreateMeetingRequest request,
        IMeetingAnalysisService analysisService,
        CancellationToken ct);

    Task<IResult> UpdateMeeting(
        Guid id,
        UpdateMeetingRequest request,
        IMeetingAnalysisService analysisService,
        CancellationToken ct);

    Task<IResult> DeleteMeeting(Guid id, CancellationToken ct);

    Task<IResult> GetStatistics(CancellationToken ct);

    Task<IResult> GenerateListPdf(IPdfService pdfService, CancellationToken ct);

    Task<IResult> GenerateMeetingPdf(Guid id, IPdfService pdfService, CancellationToken ct);
}