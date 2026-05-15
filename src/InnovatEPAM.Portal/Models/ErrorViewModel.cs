namespace InnovatEPAM.Portal.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsConcurrencyError { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
