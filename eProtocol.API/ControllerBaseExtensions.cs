using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API;

public static class ControllerBaseExtensions
{
    /// <summary>
    /// Maps a domain error to <c>404 Not Found</c> when it reports a missing entity, otherwise <c>409 Conflict</c>.
    /// </summary>
    public static ActionResult NotFoundOrConflict(this ControllerBase controller, InvalidOperationException exception)
    {
        return exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? controller.NotFound()
            : controller.Conflict(exception.Message);
    }
}
