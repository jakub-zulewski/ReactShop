using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController : BaseApiController
{
    [HttpGet("not-found")]
    public ActionResult GetNotFound()
        => NotFound();

    [HttpGet("bad-request")]
    public ActionResult GetBadRequest()
        => BadRequest(new ProblemDetails { Title = "Bad request." });

    [HttpGet("unauthorized")]
    public ActionResult GetUnauthorized()
        => Unauthorized();

    [HttpGet("validation-error")]
    public ActionResult GetValidationError()
    {
        ModelState.AddModelError("FirstKey", "First error.");
        ModelState.AddModelError("SecondKey", "Second error.");

        return ValidationProblem();
    }

    [HttpGet("server-error")]
    public ActionResult GetServerError()
        => throw new Exception("Something went wrong.");
}
