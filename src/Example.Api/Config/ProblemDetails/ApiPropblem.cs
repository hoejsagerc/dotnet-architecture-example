using ErrorOr;

namespace Example.Api.Config.ProblemDetails;

public class Problem
{
    public static IResult Response(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return TypedResults.Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return TypedResults.ValidationProblem(errors.ToDictionary(error => error.Code,
                error => new string[] { error.Description }));
        }

        var statusCode = errors[0].Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError,
        };

        return TypedResults.Problem(errors[0].Description, null, statusCode, errors[0].Code);
    }
}