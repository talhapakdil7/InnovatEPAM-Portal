using InnovatEPAM.Portal.DTOs;
using InnovatEPAM.Portal.Models;
using InnovatEPAM.Portal.Repositories.Interfaces;
using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;

namespace InnovatEPAM.Portal.Services;

/// <summary>
/// Implements the full idea scoring lifecycle: submit, update, retract, and aggregate calculation.
/// All aggregate logic lives here — no SQL computed columns or triggers.
/// </summary>
public class ScoreService : IScoreService
{
    private const string AnonymousReviewer = "Anonymous Reviewer";

    private readonly IIdeaScoreRepository _scoreRepo;
    private readonly IIdeaRepository _ideaRepo;
    private readonly ILogger<ScoreService> _logger;

    public ScoreService(
        IIdeaScoreRepository scoreRepo,
        IIdeaRepository ideaRepo,
        ILogger<ScoreService> logger)
    {
        _scoreRepo = scoreRepo;
        _ideaRepo = ideaRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SubmitScoreAsync(Guid ideaId, Guid adminId, SubmitScoreViewModel vm)
    {
        var idea = await _ideaRepo.GetByIdAsync(ideaId)
            ?? throw new InvalidOperationException($"Idea not found: {ideaId}.");

        if (idea.Status is IdeaStatus.Draft or IdeaStatus.Accepted or IdeaStatus.Rejected)
            throw new InvalidOperationException(
                $"Scoring is not allowed in status '{idea.Status}'. " +
                "Only ideas in Submitted or In review can be scored.");

        var score = new IdeaScore
        {
            IdeaId = ideaId,
            AdminId = adminId,
            Innovation = vm.Innovation,
            TechnicalFeasibility = vm.TechnicalFeasibility,
            BusinessImpact = vm.BusinessImpact,
            ImplementationValue = vm.ImplementationValue,
            SubmittedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        await _scoreRepo.UpsertAsync(score);

        // First score moves the idea from triage (Submitted) to active review (UnderReview).
        if (idea.Status == IdeaStatus.Submitted)
        {
            idea.Status = IdeaStatus.UnderReview;
            idea.LastModifiedDate = DateTime.UtcNow;
            
            try
            {
                await _ideaRepo.UpdateAsync(idea);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Concurrency error updating idea status during scoring for idea {IdeaId}: {Message}", 
                    ideaId, ex.Message);
                throw new InvalidOperationException(
                    $"Could not complete the scoring operation due to concurrent modifications. " +
                    $"Details: {ex.Message}", ex);
            }
        }

        _logger.LogInformation(
            "Admin {AdminId} submitted/updated score for idea {IdeaId}",
            adminId, ideaId);
    }

    /// <inheritdoc/>
    public async Task RetractScoreAsync(Guid ideaId, Guid adminId)
    {
        await _scoreRepo.DeleteAsync(ideaId, adminId);

        _logger.LogInformation(
            "Admin {AdminId} retracted score for idea {IdeaId}",
            adminId, ideaId);
    }

    /// <inheritdoc/>
    public async Task<ScoreSummaryDTO> GetScoreSummaryAsync(Guid ideaId, bool isBlindReviewActive)
    {
        var rows = await _scoreRepo.GetAllForIdeaAsync(ideaId);

        if (rows.Count == 0)
            return new ScoreSummaryDTO();

        decimal? AvgOf(Func<IdeaScore, int?> selector)
        {
            var values = rows.Select(selector).Where(v => v.HasValue).Select(v => (decimal)v!.Value).ToList();
            return values.Count > 0 ? Math.Round(values.Average(), 2) : null;
        }

        var avgInnovation = AvgOf(r => r.Innovation);
        var avgTF = AvgOf(r => r.TechnicalFeasibility);
        var avgBI = AvgOf(r => r.BusinessImpact);
        var avgIV = AvgOf(r => r.ImplementationValue);

        var dimAverages = new[] { avgInnovation, avgTF, avgBI, avgIV }
            .Where(v => v.HasValue).Select(v => v!.Value).ToList();

        decimal? overall = dimAverages.Count > 0
            ? Math.Round(dimAverages.Average(), 2)
            : null;

        var adminScores = rows.Select(r =>
        {
            var scoredValues = new[] { r.Innovation, r.TechnicalFeasibility, r.BusinessImpact, r.ImplementationValue }
                .Where(v => v.HasValue).Select(v => (decimal)v!.Value).ToList();

            return new AdminScoreRowDTO
            {
                AdminName = isBlindReviewActive ? AnonymousReviewer : r.Admin.FullName,
                Innovation = r.Innovation,
                TechnicalFeasibility = r.TechnicalFeasibility,
                BusinessImpact = r.BusinessImpact,
                ImplementationValue = r.ImplementationValue,
                RowAverage = scoredValues.Count > 0 ? Math.Round(scoredValues.Average(), 2) : null,
                SubmittedDate = r.LastUpdatedDate
            };
        }).ToList();

        return new ScoreSummaryDTO
        {
            ScorerCount = rows.Count,
            AvgInnovation = avgInnovation,
            AvgTechnicalFeasibility = avgTF,
            AvgBusinessImpact = avgBI,
            AvgImplementationValue = avgIV,
            OverallAverage = overall,
            AdminScores = adminScores
        };
    }

    /// <inheritdoc/>
    public async Task<IdeaScore?> GetMyScoreAsync(Guid ideaId, Guid adminId)
    {
        return await _scoreRepo.GetAsync(ideaId, adminId);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, (decimal? OverallAverage, int ScorerCount)>> GetAggregatesForIdeasAsync(
        IEnumerable<Guid> ideaIds)
    {
        var allScores = await _scoreRepo.GetBulkForIdeasAsync(ideaIds);

        return allScores
            .GroupBy(s => s.IdeaId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var rows = g.ToList();
                    var scorerCount = rows.Count;

                    decimal? AvgDim(Func<IdeaScore, int?> sel)
                    {
                        var vals = rows.Select(sel).Where(v => v.HasValue).Select(v => (decimal)v!.Value).ToList();
                        return vals.Count > 0 ? vals.Average() : (decimal?)null;
                    }

                    var dimAvgs = new[]
                    {
                        AvgDim(r => r.Innovation),
                        AvgDim(r => r.TechnicalFeasibility),
                        AvgDim(r => r.BusinessImpact),
                        AvgDim(r => r.ImplementationValue)
                    }.Where(v => v.HasValue).Select(v => v!.Value).ToList();

                    decimal? overall = dimAvgs.Count > 0
                        ? Math.Round(dimAvgs.Average(), 2)
                        : null;

                    return (overall, scorerCount);
                });
    }

    /// <inheritdoc/>
    public async Task<HashSet<Guid>> GetIdeaIdsScoredByAdminAsync(Guid adminId, IEnumerable<Guid> ideaIds)
    {
        var ids = ideaIds.Distinct().ToList();
        if (ids.Count == 0)
            return new HashSet<Guid>();

        var rows = await _scoreRepo.GetBulkForIdeasAsync(ids);
        return rows.Where(r => r.AdminId == adminId).Select(r => r.IdeaId).ToHashSet();
    }
}
