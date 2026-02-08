using MeetingIntelli.DTO.Common;

namespace MeetingIntelli.Interface;

public interface IMeetingAnalysisService
{
Task<MeetingAnalysisResult> AnalyzeMeetingAsync(string notes,string attendees, CancellationToken ct=default);
}

