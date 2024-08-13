using BuildingManager.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
namespace BuildingManager.ActionFilter
{

    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                string newMessage;

                if (errors != null) 
                {
                    var newItem = errors.SelectMany(x => x.Value).ToList();
                    newMessage = newItem[0];
           
                 } else { newMessage = "Validation error"; };

                var respObj = new ErrorResponse<object>
                {
                    Message = newMessage,
                    Error = errors
                };

                context.Result = new UnprocessableEntityObjectResult(respObj);

                //context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            }           
        }
    }
}
