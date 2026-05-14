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
}
