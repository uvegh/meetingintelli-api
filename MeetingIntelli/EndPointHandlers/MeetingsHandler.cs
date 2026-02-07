using AutoMapper;
using MeetingIntelli.Contracts;
using MeetingIntelli.DTO.Requests;
using MeetingIntelli.DTO.Responses;

namespace MeetingIntelli.EndPointHandlers;

public class MeetingsHandler : IMeetings
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MeetingsHandler> _logger;

    public MeetingsHandler(
        AppDbContext context,
        IMapper mapper,
        ILogger<MeetingsHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IResult> GetAllMeetings(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all meetings");

        var meetings = await _context.Meetings
            .OrderByDescending(m => m.MeetingDate)
            .ToListAsync(cancellationToken);

        var response = _mapper.Map<List<MeetingResponse>>(meetings);

        return Results.Ok(ApiResponse<List<MeetingResponse>>.SuccessResponse(response));
    }

    public async Task<IResult> GetMeetingById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching meeting {MeetingId}", id);

        var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found",null));
        }

        var response = _mapper.Map<MeetingResponse>(meeting);

        return Results.Ok(ApiResponse<MeetingResponse>.SuccessResponse(response));
    }

    public async Task<IResult> CreateMeeting(
        CreateMeetingRequest request,
        IMeetingAnalysisService analysisService,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new meeting: {Title}", request.Title);

        try
        {
            // Analyze meeting with AI
            var analysis = await analysisService.AnalyzeMeetingAsync(
                request.Notes,
                request.Attendees,
                cancellationToken
            );

            // Map DTO to entity
            var meeting = _mapper.Map<Meeting>(request);
            meeting.Summary = analysis.Summary;
            meeting.ActionItemsJson = JsonSerializer.Serialize(analysis.ActionItems);
            meeting.CreatedAt = DateTime.UtcNow;

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created meeting {MeetingId}", meeting.Id);

            var response = _mapper.Map<MeetingResponse>(meeting);

            return Results.CreatedAtRoute(
                "GetMeetingById",
                new { id = meeting.Id },
                ApiResponse<MeetingResponse>.SuccessResponse(response, "Meeting created successfully")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meeting");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to create meeting",
                new List<string> { ex.Message }
            ));
        }
    }

    public async Task<IResult> UpdateMeeting(
        Guid id,
        UpdateMeetingRequest request,
        IMeetingAnalysisService analysisService,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating meeting {MeetingId}", id);

        var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for update", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found",null));
        }

        try
        {
            var notesChanged = meeting.Notes != request.Notes;

            // Map request to existing entity
            _mapper.Map(request, meeting);

            if (notesChanged)
            {
                _logger.LogInformation("Notes changed, re-analyzing meeting {MeetingId}", id);

                var analysis = await analysisService.AnalyzeMeetingAsync(
                    request.Notes,
                    request.Attendees,
                    cancellationToken
                );

                meeting.Summary = analysis.Summary;
                meeting.ActionItemsJson = JsonSerializer.Serialize(analysis.ActionItems);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated meeting {MeetingId}", id);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meeting {MeetingId}", id);
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to update meeting",
                new List<string> { ex.Message }
            ));
        }
    }

    public async Task<IResult> DeleteMeeting(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting meeting {MeetingId}", id);

        var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for deletion", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found",null));
        }

        _context.Meetings.Remove(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted meeting {MeetingId}", id);

        return Results.NoContent();
    }

    public async Task<IResult> GetStatistics(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching meeting statistics");

        var meetings = await _context.Meetings.ToListAsync(cancellationToken);

        var byMonth = meetings
            .GroupBy(m => new { m.MeetingDate.Year, m.MeetingDate.Month })
            .Select(g => new MonthlyCount
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToList();

        var statistics = new MeetingStatisticsResponse
        {
            ByMonth = byMonth
        };

        return Results.Ok(ApiResponse<MeetingStatisticsResponse>.SuccessResponse(statistics));
    }

    public async Task<IResult> GenerateListPdf(
        IPdfService pdfService,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating PDF for meetings list");

        try
        {
            var pdf = await pdfService.GeneratePdfFromUrlAsync(
                "/meetings?print=true",
                cancellationToken
            );

            var filename = $"meetings-overview-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

            return Results.File(pdf, "application/pdf", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate list PDF");
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to generate PDF"
            );
        }
    }

    public async Task<IResult> GenerateMeetingPdf(
        Guid id,
        IPdfService pdfService,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating PDF for meeting {MeetingId}", id);

        var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);
        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for PDF generation", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found",null));
        }

        try
        {
            var pdf = await pdfService.GeneratePdfFromUrlAsync(
                $"/meetings/{id}?print=true",
                cancellationToken
            );

            var filename = $"meeting-{meeting.Title.Replace(" ", "-")}-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

            return Results.File(pdf, "application/pdf", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for meeting {MeetingId}", id);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to generate PDF"
            );
        }
    }
}