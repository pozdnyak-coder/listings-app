using FluentValidation;

namespace ListingsApi.Validators;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        // аргументы endpoint’а уже разобраны из JSON — берём наш DTO
        var request = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
            return TypedResults.Problem("Пустое тело запроса", statusCode: 400);

        // только Async: в валидаторе есть MustAsync
        var result = await validator.ValidateAsync(request, ctx.HttpContext.RequestAborted);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        return await next(ctx);
    }
}
