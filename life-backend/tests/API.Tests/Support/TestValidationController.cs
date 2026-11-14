using FluentValidation;
using Life.API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Life.API.Tests.Support;

public sealed record CreateSampleRequest(string Name, int Quantity);

public sealed record SampleResponse(string Name, int Quantity);

public sealed class CreateSampleRequestValidator : AbstractValidator<CreateSampleRequest>
{
    public CreateSampleRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Quantity).InclusiveBetween(1, 100);
    }
}

public class TestValidationController : ApiControllerBase
{
    [HttpPost]
    public ActionResult<SampleResponse> Create([FromBody] CreateSampleRequest request)
    {
        return Ok(new SampleResponse(request.Name, request.Quantity));
    }
}
