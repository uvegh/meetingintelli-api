using MeetingIntelli.Contracts;
using MeetingIntelli.DTO.Requests;
using MeetingIntelli.DTO.Responses;
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
            IMeetingAnalysisService analysisService,
            CancellationToken ct) => await handler.CreateMeeting(request, analysisService, ct))
            .WithName("CreateMeeting")
            .Produces<ApiResponse<MeetingResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        // PUT /api/meetings/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMeetingRequest request,
            IMeetings handler,
            IMeetingAnalysisService analysisService,
            CancellationToken ct) => await handler.UpdateMeeting(id, request, analysisService, ct))
            .WithName("UpdateMeeting")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

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
            .Produces<FileContentResult>(StatusCodes.Status200OK);

        // POST /api/meetings/{id}/pdf
                  group.MapPost("/{id:guid}/pdf", async (
            Guid id,
            IMeetings handler,
            IPdfService pdfService,
            CancellationToken ct) => await handler.GenerateMeetingPdf(id, pdfService, ct))
            .WithName("GenerateMeetingPdf")
            .Produces<FileContentResult>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return app;
    }
}