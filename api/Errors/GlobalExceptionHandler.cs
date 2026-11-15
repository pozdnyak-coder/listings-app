using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ListingsApi.Errors;

// Вызывается для любого исключения, которое никто не поймал
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "Необработанная ошибка {Method} {Path}", http.Request.Method, http.Request.Path);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Внутренняя ошибка сервера",
            Detail = env.IsDevelopment() ? ex.Message : null,   // наружу — только разработчику
            Instance = http.Request.Path,
        };
        problem.Extensions["traceId"] = http.TraceIdentifier;   // по нему найдём строку в логе

        http.Response.StatusCode = problem.Status.Value;
        await http.Response.WriteAsJsonAsync(problem, ct);
        return true;                                            // обработали
    }
}
