using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StandardErrorMessages
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext ctx)
		{
			if (!ctx.ModelState.IsValid)
			{
				ctx.Result = new JsonResult(ErrorResponse.InvalidModel(ctx.ModelState));
			}
		}
	}
}