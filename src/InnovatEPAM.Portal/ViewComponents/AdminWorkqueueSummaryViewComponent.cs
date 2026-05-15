using InnovatEPAM.Portal.Services.Interfaces;
using InnovatEPAM.Portal.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InnovatEPAM.Portal.ViewComponents;

/// <summary>Real workqueue counts for admin topbar notifications (domain-driven).</summary>
public class AdminWorkqueueSummaryViewComponent : ViewComponent
{
    private readonly IIdeaService _ideaService;

    public AdminWorkqueueSummaryViewComponent(IIdeaService ideaService)
    {
        _ideaService = ideaService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (HttpContext.User?.IsInRole("Admin") != true)
            return Content(string.Empty);

        var ideas = await _ideaService.GetAllIdeasAsync(null);
        var triage = ideas.Count(i => i.Status == "Submitted");
        var inPipeline = ideas.Count(i => i.Status == "UnderReview");

        return View(new AdminWorkqueueSummaryVm
        {
            TriageCount = triage,
            InPipelineCount = inPipeline
        });
    }
}
