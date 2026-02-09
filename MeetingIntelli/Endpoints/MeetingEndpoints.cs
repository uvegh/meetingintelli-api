using MeetingIntelli.Contracts;
using MeetingIntelli.DTO.Requests;
using MeetingIntelli.DTO.Responses;
using MeetingIntelli.Interface;
using MeetingIntelli.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetingIntelli.Endpoints;

public static class MeetingEndpoints
{
    public static IEndpointRouteBuilder MapMeetingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/meetings")
            .WithTags("Meetings")
            .WithOpenApi();

        // GET /api/meetings
        group.MapGet("/", async (
            IMeetings handler,
            CancellationToken ct) => await handler.GetAllMeetings(ct))
            .WithName("GetAllMeetings")
            .Produces<ApiResponse<List<MeetingResponse>>>(StatusCodes.Status200OK);

        // GET /api/meetings/{id}
group.MapGet("/{id:guid}", async (
            Guid id,
            IMeetings handler,
            CancellationToken ct) => await handler.GetMeetingById(id, ct))
            .WithName("GetMeetingById")
            .Produces<ApiResponse<MeetingResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        // POST /api/meetings
               group.MapPost("/", async (
            CreateMeetingRequest request,
            IMeetings handler,
            CancellationToken ct) => await handler.CreateMeetingAsync(request, ct))
            .WithName("CreateMeeting")
            .Produces<ApiResponse<MeetingResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // PUT /api/meetings/{id}
    group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMeetingRequest request,
            IMeetings handler,
            CancellationToken ct) => await handler.UpdateMeeting(id, request, ct))
            .WithName("UpdateMeeting")
            .Produces<ApiResponse<MeetingResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // DELETE /api/meetings/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMeetings handler,
            CancellationToken ct) => await handler.DeleteMeeting(id, ct))
            .WithName("DeleteMeeting")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        // GET /api/meetings/statistics
        group.MapGet("/statistics", async (
            IMeetings handler,
            CancellationToken ct) => await handler.GetStatistics(ct))
            .WithName("GetStatistics")
            .Produces<ApiResponse<MeetingStatisticsResponse>>(StatusCodes.Status200OK);

        // POST /api/meetings/pdf/list
        group.MapPost("/pdf/list", async (
            IMeetings handler,
            IPdfService pdfService,
            CancellationToken ct) => await handler.GenerateListPdf(pdfService, ct))
            .WithName("GenerateListPdf")
            .Produces<FileContentResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // POST /api/meetings/{id}/pdf
     group.MapPost("/{id:guid}/pdf", async ( Guid id,
            IMeetings handler,
            IPdfService pdfService,
            CancellationToken ct) => await handler.GenerateMeetingPdf(id, pdfService, ct))
            .WithName("GenerateMeetingPdf")
            .Produces<FileContentResult>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


        group.MapPost("/test-ai", async (
    IMeetingAnalysisService analysisService,
    CancellationToken ct) =>
        {
            var testNotes = @"
Team retrospective meeting held today.

What went well:
- Launched new Claude response feature on time
- Great collaboration between teams

What didn't go well:
- Too many meetings slowing down productivity
- Requirements were unclear, causing rework

Decisions made:
- Reduce meeting frequency
- Product team will provide clearer specs

Action items:
- John Smith will send out new meeting schedule by Friday
- Sarah Johnson needs to create requirement template by end of next week
- Mike Chen should review all PRs with unclear requirements starting today
- Team to schedule follow-up retrospective first Friday of March
";

            var testAttendees = "John Smith, Sarah Johnson, Mike Chen";

            var result = await analysisService.AnalyzeMeetingAsync(
                testNotes,
                testAttendees,
                ct);

            return Results.Ok(new
            {
                result.Summary,
                ActionItemCount = result.ActionItems.Count,
                result.ActionItems
            });
        })
.WithName("TestAI");

        return app;
    }
}