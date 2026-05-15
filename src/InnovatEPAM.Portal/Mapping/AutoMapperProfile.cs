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
            .ForMember(d => d.CurrentReviewStageName, o => o.MapFrom(s => ""))
            .ForMember(d => d.CurrentReviewStageOrder, o => o.MapFrom(s => 0))
            .ForMember(d => d.AttachmentCount, o => o.MapFrom(s => s.IdeaAttachments.Count))
            .ForMember(d => d.AggregateScore, o => o.Ignore())
            .ForMember(d => d.ScorerCount, o => o.Ignore())
            .ForMember(d => d.CanDeleteAsOwner, o => o.MapFrom(s => SubmitOwnerMayDelete(s)))
            .ForMember(d => d.DeleteBlockedHint, o => o.MapFrom(s => SubmitDeleteBlockedHint(s)));

        CreateMap<Idea, IdeaDetailDTO>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SubmitterName, o => o.MapFrom(s => s.Submitter.FullName))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
            .ForMember(d => d.CategoryDisplayName, o => o.Ignore())
            .ForMember(d => d.CategoryDataFields, o => o.Ignore())
            .ForMember(d => d.StageHistory, o => o.Ignore())
            .ForMember(d => d.ScoreSummary, o => o.Ignore())
            .ForMember(d => d.MyScore, o => o.Ignore())
            .ForMember(d => d.CanAmendSubmitted, o => o.MapFrom(s => s.Status == IdeaStatus.Submitted && s.Scores.Count == 0))
            .ForMember(d => d.CanDeleteAsOwner, o => o.MapFrom(s => SubmitOwnerMayDelete(s)))
            .ForMember(d => d.DeleteBlockedHint, o => o.MapFrom(s => SubmitDeleteBlockedHint(s)));

        CreateMap<IdeaAttachment, IdeaAttachmentDTO>();

        CreateMap<AuditLog, AuditLogDTO>()
            .ForMember(d => d.ChangedByAdmin, o => o.MapFrom(s => s.ChangedByAdmin.FullName));
    }

    private static bool SubmitOwnerMayDelete(Idea s) =>
        s.Status == IdeaStatus.Draft
        || s.Status == IdeaStatus.Accepted
        || s.Status == IdeaStatus.Rejected
        || (s.Status == IdeaStatus.Submitted && s.Scores.Count == 0);

    private static string? SubmitDeleteBlockedHint(Idea s)
    {
        if (s.Status == IdeaStatus.UnderReview)
            return "This submission cannot be removed while it is under review.";
        if (s.Status == IdeaStatus.Submitted && s.Scores.Count > 0)
            return "This submission can no longer be withdrawn.";
        return null;
    }
}

