using AutoMapper;
using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Idea, IdeaListItemDTO>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SubmitterName, o => o.MapFrom(s => s.Submitter.FullName));

        CreateMap<Idea, IdeaDetailDTO>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SubmitterName, o => o.MapFrom(s => s.Submitter.FullName));

        CreateMap<IdeaAttachment, IdeaAttachmentDTO>();

        CreateMap<AuditLog, AuditLogDTO>()
            .ForMember(d => d.ChangedByAdmin, o => o.MapFrom(s => s.ChangedByAdmin.FullName));
    }
}
