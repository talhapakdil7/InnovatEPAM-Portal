using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Services;

/// <summary>
/// Implements the multi-stage innovation review pipeline.
/// All business rules for advancing, reverting, and finalising an idea's review stage live here.
/// </summary>
public class ReviewWorkflowService : IReviewWorkflowService
{
    private readonly IIdeaRepository _ideaRepo;
    private readonly IStageTransitionRepository _transitionRepo;
    private readonly ILogger<ReviewWorkflowService> _logger;

    public ReviewWorkflowService(
        IIdeaRepository ideaRepo,
        IStageTransitionRepository transitionRepo,
        ILogger<ReviewWorkflowService> logger)
    {
        _ideaRepo = ideaRepo;
        _transitionRepo = transitionRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> AdvanceStageAsync(
        Guid ideaId, Guid adminId, AdvanceStageViewModel vm)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null)
            return (false, "Idea not found.");

        // Only ideas that are Under Review or have already entered the pipeline can be advanced.
        if (idea.Status == IdeaStatus.Draft || idea.Status == IdeaStatus.Submitted)
            return (false, "Idea must be Under Review before stage advancement.");

        // Final Decision stage cannot be advanced further.
        if (idea.CurrentReviewStage == ReviewStage.FinalDecision)
            return (false, "Idea is already at the Final Decision stage.");

        var fromStage = idea.CurrentReviewStage;
        var toStage = fromStage.HasValue
            ? ReviewStageHelper.NextStage(fromStage.Value)
            : ReviewStage.InitialScreening;

        if (toStage == null)
            return (false, "No next stage available.");

        idea.CurrentReviewStage = toStage;
        if (idea.Status == IdeaStatus.Submitted)
            idea.Status = IdeaStatus.UnderReview;

        idea.LastModifiedDate = DateTime.UtcNow;
        await _ideaRepo.UpdateAsync(idea);

        await _transitionRepo.AddAsync(new StageTransition
        {
            Id = Guid.NewGuid(),
            IdeaId = ideaId,
            FromStage = fromStage,
            ToStage = toStage.Value,
            IsAdvance = true,
            Notes = vm.Notes,
            TransitionedByAdminId = adminId,
            TransitionDate = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Idea {IdeaId} advanced from {From} to {To} by admin {AdminId}",
            ideaId, fromStage?.ToString() ?? "none", toStage.ToString(), adminId);

        return (true, null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> RevertStageAsync(
        Guid ideaId, Guid adminId, RevertStageViewModel vm)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null)
            return (false, "Idea not found.");

        if (idea.CurrentReviewStage == null)
            return (false, "Idea has not yet entered the review pipeline.");

        if (ReviewStageHelper.IsFirstStage(idea.CurrentReviewStage.Value))
            return (false, "Idea is already at the first stage and cannot be reverted further.");

        var fromStage = idea.CurrentReviewStage.Value;
        var toStage = (ReviewStage)((int)fromStage - 1);

        idea.CurrentReviewStage = toStage;
        idea.LastModifiedDate = DateTime.UtcNow;
        await _ideaRepo.UpdateAsync(idea);

        await _transitionRepo.AddAsync(new StageTransition
        {
            Id = Guid.NewGuid(),
            IdeaId = ideaId,
            FromStage = fromStage,
            ToStage = toStage,
            IsAdvance = false,
            Notes = vm.Notes,
            RevertReason = vm.RevertReason,
            TransitionedByAdminId = adminId,
            TransitionDate = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Idea {IdeaId} reverted from {From} to {To} by admin {AdminId}. Reason: {Reason}",
            ideaId, fromStage, toStage, adminId, vm.RevertReason);

        return (true, null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> RecordDecisionAsync(
        Guid ideaId, Guid adminId, RecordDecisionViewModel vm)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId);
        if (idea == null)
            return (false, "Idea not found.");

        if (idea.CurrentReviewStage != ReviewStage.FinalDecision)
            return (false, "Idea must be at the Final Decision stage to record a decision.");

        if (vm.Outcome != "Accepted" && vm.Outcome != "Rejected")
            return (false, "Outcome must be 'Accepted' or 'Rejected'.");

        idea.Status = vm.Outcome == "Accepted" ? IdeaStatus.Accepted : IdeaStatus.Rejected;
        idea.LastModifiedDate = DateTime.UtcNow;
        await _ideaRepo.UpdateAsync(idea);

        await _transitionRepo.AddAsync(new StageTransition
        {
            Id = Guid.NewGuid(),
            IdeaId = ideaId,
            FromStage = ReviewStage.FinalDecision,
            ToStage = ReviewStage.FinalDecision,
            IsAdvance = true,
            Notes = vm.Notes,
            Outcome = vm.Outcome,
            TransitionedByAdminId = adminId,
            TransitionDate = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Final decision '{Outcome}' recorded for idea {IdeaId} by admin {AdminId}",
            vm.Outcome, ideaId, adminId);

        return (true, null);
    }
}
