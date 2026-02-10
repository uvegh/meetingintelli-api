using AutoMapper;
using MeetingIntelli.Contracts;

using MeetingIntelli.DTO.Requests;
using MeetingIntelli.DTO.Responses;

using Microsoft.EntityFrameworkCore;

namespace MeetingIntelli.EndPointHandlers;

public class MeetingsHandler : IMeetings
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MeetingsHandler> _logger;
    private readonly IMeetingAnalysisService _meetingAnalysisService;

    public MeetingsHandler(
        AppDbContext context,
        IMapper mapper,
        IMeetingAnalysisService meetingAnalysisService,
        ILogger<MeetingsHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _meetingAnalysisService = meetingAnalysisService ?? throw new ArgumentNullException(nameof(meetingAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IResult> GetAllMeetings(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all meetings");

        var meetings = await _context.Meetings
            .Include(m => m.ActionItems) // ← Load action items
            .OrderByDescending(m => m.MeetingDate)
            .ToListAsync(cancellationToken);

        var response = _mapper.Map<List<MeetingResponse>>(meetings);

        return Results.Ok(ApiResponse<List<MeetingResponse>>.SuccessResponse(response));
    }

    public async Task<IResult> GetMeetingById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching meeting {MeetingId}", id);

        var meeting = await _context.Meetings
            .Include(m => m.ActionItems) 
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found", null));
        }

        var response = _mapper.Map<MeetingResponse>(meeting);

        return Results.Ok(ApiResponse<MeetingResponse>.SuccessResponse(response));
    }

    public async Task<IResult> CreateMeetingAsync(
        CreateMeetingRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new meeting: {Title}", request.Title);

        try
        {
            // Analyze notes with Claude AI
            var analysis = await _meetingAnalysisService.AnalyzeMeetingAsync(
                request.Notes,
                request.Attendees,
                cancellationToken);

            _logger.LogDebug("Claude analysis complete. Summary length: {Length}, Action items: {Count}",
                analysis.Summary?.Length ?? 0,
                analysis.ActionItems?.Count ?? 0);

            // Create meeting entity
            var meeting = new Meeting
            {
                Title = request.Title,
                MeetingDate = request.MeetingDate,
                Attendees = request.Attendees,
                Notes = request.Notes,
                Summary = analysis.Summary,
                ActionItems = analysis.ActionItems?.Select(dto => new ActionItem
                {
                    Assignee = dto.Assignee,
                    Task = dto.Task,
                    DueDate = dto.DueDate,
                    Priority = dto.Priority
                }).ToList() ?? new List<ActionItem>()
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Meeting created with ID: {Id}, Action items: {Count}",
                meeting.Id,
                meeting.ActionItems.Count);

            // Map to response
            var response = _mapper.Map<MeetingResponse>(meeting);

            return Results.CreatedAtRoute(
                "GetMeetingById",
                new { id = meeting.Id },
                ApiResponse<MeetingResponse>.SuccessResponse(response, "Meeting created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meeting");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to create meeting",
                new List<string> { ex.Message }));
        }
    }




    //public async Task<IResult> UpdateMeeting(
    //Guid id,
    //UpdateMeetingRequest request,
    //CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Updating meeting {MeetingId}", id);

    //    var meeting = await _context.Meetings
    //        .Include(m => m.ActionItems)
    //        .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    //    if (meeting == null)
    //    {
    //        _logger.LogWarning("Meeting {MeetingId} not found for update", id);
    //        return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found", null));
    //    }

    //    try
    //    {
    //        var notesChanged = meeting.Notes != request.Notes;

    //        // Update basic fields
    //        meeting.Title = request.Title;
    //        meeting.MeetingDate = request.MeetingDate;
    //        meeting.Attendees = request.Attendees;
    //        meeting.Notes = request.Notes;
    //        meeting.UpdatedAt = DateTime.UtcNow;

    //        if (notesChanged)
    //        {
    //            _logger.LogInformation("Notes changed, re-analyzing meeting {MeetingId}", id);

    //            var analysis = await _meetingAnalysisService.AnalyzeMeetingAsync(
    //                request.Notes,
    //                request.Attendees,
    //                cancellationToken);

    //            meeting.Summary = analysis.Summary;


    //            var existingActionItems = meeting.ActionItems.ToList();
    //            foreach (var item in existingActionItems)
    //            {
    //                _context.ActionItems.Remove(item);
    //            }

    //            // Clear action collection
    //            meeting.ActionItems.Clear();

    //            // Add new action items
    //            if (analysis.ActionItems != null && analysis.ActionItems.Any())
    //            {
    //                foreach (var dto in analysis.ActionItems)
    //                {
    //                    meeting.ActionItems.Add(new ActionItem
    //                    {

    //                        MeetingId = meeting.Id,
    //                        Assignee = dto.Assignee,
    //                        Task = dto.Task,
    //                        DueDate = dto.DueDate,
    //                        Priority = dto.Priority
    //                    });
    //                }
    //            }
    //        }

    //        await _context.SaveChangesAsync(cancellationToken);

    //        _logger.LogInformation("Successfully updated meeting {MeetingId}", id);

    //        // Reload to get the new action items with their IDs
    //        await _context.Entry(meeting)
    //            .Collection(m => m.ActionItems)
    //            .LoadAsync(cancellationToken);

    //        var response = _mapper.Map<MeetingResponse>(meeting);
    //        return Results.Ok(ApiResponse<MeetingResponse>.SuccessResponse(response));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating meeting {MeetingId}", id);
    //        return Results.BadRequest(ApiResponse<object>.ErrorResponse(
    //            "Failed to update meeting",
    //            new List<string> { ex.Message }));
    //    }
    //}

    public async Task<IResult> UpdateMeeting(
    Guid id,
    UpdateMeetingRequest request,
    CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating meeting {MeetingId}", id);

        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for update", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found", null));
        }

        try
        {
            var notesChanged = meeting.Notes != request.Notes;

            // Update basic fields
            meeting.Title = request.Title;
            meeting.MeetingDate = request.MeetingDate;
            meeting.Attendees = request.Attendees;
            meeting.Notes = request.Notes;
            meeting.UpdatedAt = DateTime.UtcNow;

            if (notesChanged)
            {
                _logger.LogInformation("Notes changed, re-analyzing meeting {MeetingId}", id);

                var analysis = await _meetingAnalysisService.AnalyzeMeetingAsync(
                    request.Notes,
                    request.Attendees,
                    cancellationToken);

                meeting.Summary = analysis.Summary;

            
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM action_items WHERE meeting_id = {id}",
                    cancellationToken);

               
                if (analysis.ActionItems != null && analysis.ActionItems.Any())
                {
                    var newActionItems = analysis.ActionItems.Select(dto => new ActionItem
                    {
                        MeetingId = meeting.Id,
                        Assignee = dto.Assignee,
                        Task = dto.Task,
                        DueDate = dto.DueDate,
                        Priority = dto.Priority
                    }).ToList();

                    _context.ActionItems.AddRange(newActionItems);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated meeting {MeetingId}", id);

            // Reload with action items
            var updatedMeeting = await _context.Meetings
                .Include(m => m.ActionItems)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            var response = _mapper.Map<MeetingResponse>(updatedMeeting);
            return Results.Ok(ApiResponse<MeetingResponse>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meeting {MeetingId}", id);
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                "Failed to update meeting",
                new List<string> { ex.Message }));
        }
    }

    public async Task<IResult> DeleteMeeting(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting meeting {MeetingId}", id);

        var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for deletion", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found", null));
        }

        _context.Meetings.Remove(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted meeting {MeetingId}", id);

        return Results.NoContent();
    }

    public async Task<IResult> GetStatistics(CancellationToken cancellationToken)
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

    //public async Task<IResult> GenerateListPdf(
    //    IPdfService pdfService,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Generating PDF for meetings list");

    //    try
    //    {
    //        var pdf = await pdfService.GeneratePdfFromUrlAsync(
    //            "/meetings?print=true",
    //            cancellationToken);

    //        var filename = $"meetings-overview-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

    //        return Results.File(pdf, "application/pdf", filename);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to generate list PDF");
    //        return Results.Problem(
    //            detail: ex.Message,
    //            statusCode: StatusCodes.Status500InternalServerError,
    //            title: "Failed to generate PDF");
    //    }
    //}

    //public async Task<IResult> GenerateMeetingPdf(
    //    Guid id,
    //    IPdfService pdfService,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Generating PDF for meeting {MeetingId}", id);

    //    var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);

    //    if (meeting == null)
    //    {
    //        _logger.LogWarning("Meeting {MeetingId} not found for PDF generation", id);
    //        return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found", null));
    //    }

    //    try
    //    {
    //        var pdf = await pdfService.GeneratePdfFromUrlAsync(
    //            $"/meetings/{id}?print=true",
    //            cancellationToken);

    //        var filename = $"meeting-{meeting.Title.Replace(" ", "-")}-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

    //        return Results.File(pdf, "application/pdf", filename);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to generate PDF for meeting {MeetingId}", id);
    //        return Results.Problem(
    //            detail: ex.Message,
    //            statusCode: StatusCodes.Status500InternalServerError,
    //            title: "Failed to generate PDF");
    //    }
    //}

    public async Task<IResult> GenerateListPdf(
    IPdfService pdfService,
    int? viewportWidth,
    int? viewportHeight,
    CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating PDF for meetings list with viewport {Width}x{Height}",
            viewportWidth ?? 1920, viewportHeight ?? 1080);

        try
        {
            var pdf = await pdfService.GeneratePdfFromUrlAsync(
                "/meetings?print=true",
                viewportWidth,
                viewportHeight,
                cancellationToken);

            var filename = $"meetings-overview-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

            return Results.File(pdf, "application/pdf", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate list PDF");
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to generate PDF");
        }
    }

    public async Task<IResult> GenerateMeetingPdf(
        Guid id,
        IPdfService pdfService,
        int? viewportWidth,
        int? viewportHeight,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating PDF for meeting {MeetingId} with viewport {Width}x{Height}",
            id, viewportWidth ?? 1920, viewportHeight ?? 1080);

        var meeting = await _context.Meetings.FindAsync(new object[] { id }, cancellationToken);

        if (meeting == null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for PDF generation", id);
            return Results.NotFound(ApiResponse<object>.ErrorResponse($"Meeting {id} not found", null));
        }

        try
        {
            var pdf = await pdfService.GeneratePdfFromUrlAsync(
                $"/meetings/{id}?print=true",
                viewportWidth,
                viewportHeight,
                cancellationToken);

            var filename = $"meeting-{meeting.Title.Replace(" ", "-")}-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

            return Results.File(pdf, "application/pdf", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for meeting {MeetingId}", id);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to generate PDF");
        }
    }
}