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
            .ForMember(d => d.SubmitterName, o => o.MapFrom(s => s.Submitter.FullName))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
            .ForMember(d => d.CategoryDisplayName, o => o.Ignore())
            .ForMember(d => d.CurrentReviewStageName, o => o.MapFrom(s => ReviewStageHelper.DisplayName(s.CurrentReviewStage)))
            .ForMember(d => d.CurrentReviewStageOrder, o => o.MapFrom(s => s.CurrentReviewStage.HasValue ? (int)s.CurrentReviewStage.Value : 0));

        CreateMap<Idea, IdeaDetailDTO>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SubmitterName, o => o.MapFrom(s => s.Submitter.FullName))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
            .ForMember(d => d.CategoryDisplayName, o => o.Ignore())
            .ForMember(d => d.CategoryDataFields, o => o.Ignore())
            .ForMember(d => d.CurrentReviewStageName, o => o.MapFrom(s => ReviewStageHelper.DisplayName(s.CurrentReviewStage)))
            .ForMember(d => d.CurrentReviewStageOrder, o => o.MapFrom(s => s.CurrentReviewStage.HasValue ? (int)s.CurrentReviewStage.Value : 0))
            .ForMember(d => d.IsAtFinalStage, o => o.MapFrom(s => s.CurrentReviewStage == ReviewStage.FinalDecision))
            .ForMember(d => d.StageHistory, o => o.Ignore());

        CreateMap<IdeaAttachment, IdeaAttachmentDTO>();

        CreateMap<AuditLog, AuditLogDTO>()
            .ForMember(d => d.ChangedByAdmin, o => o.MapFrom(s => s.ChangedByAdmin.FullName));

        CreateMap<StageTransition, StageTransitionDTO>()
            .ForMember(d => d.FromStageName, o => o.MapFrom(s => ReviewStageHelper.DisplayName(s.FromStage)))
            .ForMember(d => d.ToStageName, o => o.MapFrom(s => ReviewStageHelper.DisplayName(s.ToStage)))
            .ForMember(d => d.TransitionedByAdminName, o => o.MapFrom(s => s.TransitionedByAdmin.FullName));
    }
}
