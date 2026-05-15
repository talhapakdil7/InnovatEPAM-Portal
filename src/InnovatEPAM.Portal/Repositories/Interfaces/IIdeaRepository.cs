using InnovatEPAM.Portal.Models;

namespace InnovatEPAM.Portal.Repositories.Interfaces;

public interface IIdeaRepository
{
    Task<Idea?> GetByIdAsync(Guid id);
    Task<List<Idea>> GetBySubmitterAsync(Guid submitterId);
    Task<List<Idea>> GetAllAsync();
    Task<List<Idea>> GetByStatusAsync(IdeaStatus status);
    Task AddAsync(Idea idea);
    Task UpdateAsync(Idea idea);

    /// <summary>Admin list: non-draft ideas matching filters, paged, with total count.</summary>
    Task<(List<Idea> Items, int TotalCount)> GetAdminIdeasFilteredPagedAsync(
        string? statusFilter, string? categoryFilter, int skip, int take);

    /// <summary>Counts by status for all non-draft ideas (dashboard cards).</summary>
    Task<Dictionary<IdeaStatus, int>> CountNonDraftByStatusAsync();

    /// <summary>Status counts for the same filtered set as admin paging (consistent summary).</summary>
    Task<Dictionary<string, int>> GetAdminStatusCountsAsync(string? statusFilter, string? categoryFilter);

    /// <summary>Non-draft category distribution; empty key means uncategorized.</summary>
    Task<Dictionary<string, int>> CountByCategoryNonDraftAsync();

    /// <summary>Dashboard quick list — last updated N non-draft ideas.</summary>
    Task<List<Idea>> GetRecentNonDraftIdeasAsync(int take);

    /// <summary>Scoring queue: Submitted + UnderReview, scores included.</summary>
    Task<List<Idea>> GetScorableIdeasWithScoresAsync();

    /// <summary>All non-draft idea IDs (portfolio totals).</summary>
    Task<List<Guid>> GetNonDraftIdeaIdsAsync();

    /// <summary>Count of ideas in UnderReview with no score records yet.</summary>
    Task<int> CountUnderReviewWithNoScoresAsync();
}
