using AutoMapper;
using MeetingIntelli.DTO.Requests;
using MeetingIntelli.DTO.Responses;


namespace MeetingIntelli.Configurations;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        
        CreateMap<Meeting, MeetingResponse>().ReverseMap();

        CreateMap<CreateMeetingRequest, Meeting>().ReverseMap();



        CreateMap<UpdateMeetingRequest, Meeting>().ReverseMap();
        CreateMap<ActionItem, ActionItemResponse>();


        //CreateMap<Meeting, MeetingResponse>()
        //    .ForMember(dest => dest.ActionItems, opt => opt.MapFrom(src => src.ActionItems));


        CreateMap<ActionItem, ActionItemResponse>();


        //CreateMap<CreateMeetingRequest, Meeting>()
        //    .ForMember(dest => dest.Id, opt => opt.Ignore())
        //    .ForMember(dest => dest.Summary, opt => opt.Ignore())
        //    .ForMember(dest => dest.ActionItems, opt => opt.Ignore())
        //    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        //    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


        //CreateMap<UpdateMeetingRequest, Meeting>()
        //    .ForMember(dest => dest.Id, opt => opt.Ignore())
        //    .ForMember(dest => dest.Summary, opt => opt.Ignore())
        //    .ForMember(dest => dest.ActionItems, opt => opt.Ignore())
        //    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        //    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

    }


}