using AutoMapper;
using MeetingIntelli.DTO.Requests;
using MeetingIntelli.DTO.Responses;


namespace MeetingIntelli.Configurations;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        // Meeting -> MeetingResponse (for GET operations)
        CreateMap<Meeting, MeetingResponse>()
            .ForMember(dest => dest.ActionItems,
                opt => opt.MapFrom(src => DeserializeActionItems(src.ActionItemsJson)));

       
        CreateMap<CreateMeetingRequest, Meeting>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.Summary, opt => opt.Ignore()) // Comes from AI
            .ForMember(dest => dest.ActionItemsJson, opt => opt.Ignore()); // Comes from AI

      
        CreateMap<UpdateMeetingRequest, Meeting>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't overwrite
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Don't overwrite
            .ForMember(dest => dest.Summary, opt => opt.Ignore()) // Conditionally updated
            .ForMember(dest => dest.ActionItemsJson, opt => opt.Ignore()); // Conditionally updated
    }

    private static List<ActionItemResponse>? DeserializeActionItems(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<ActionItemResponse>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (JsonException)
        {
            return null;
        }
    }
}