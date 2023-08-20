using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MfaApi.Helpers;

public class InvalidModelStateResponse
{
    public string Message => "One or more validation errors occured.";

    public List<InvalidModelStateError> Errors { get; set; } = new List<InvalidModelStateError>();

    public InvalidModelStateResponse(ModelStateDictionary modelState)
    {
        var errorsInModelState = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray());

        foreach (var error in errorsInModelState)
        {
            if(error.Value != null)
            {
                foreach (var subError in error.Value)
                {
                    var errorModel = new InvalidModelStateError
                    {
                        FieldName = error.Key,
                        Message = subError
                    };

                    Errors.Add(errorModel);
                }
            }
        }
    }
}

public class InvalidModelStateError
{
    public required string FieldName { get; set; }
    public required string Message { get; set; }
}
