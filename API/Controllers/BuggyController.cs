using Microsoft.AspNetCore.Mvc;
using API.Errors;
using Infrastructure.Data;
using API.DTOs;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly StoreContext _context;

        public BuggyController(StoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Test endpoint to generate a 404 Not Found response
        /// </summary>
        [HttpGet("notfound")]
        public ActionResult GetNotFoundRequest()
        {
            var thing = _context.Products.Find(999);
            
            if (thing == null)
            {
                return NotFound(new ApiErrorResponse(404));
            }

            return Ok(thing);
        }

        /// <summary>
        /// Test endpoint to generate a 500 Server Error
        /// </summary>
        [HttpGet("servererror")]
        public ActionResult GetServerError()
        {
            var thing = _context.Products.Find(999);
            var thingToReturn = thing!.ToString(); // This will throw NullReferenceException

            return Ok(thingToReturn);
        }

        /// <summary>
        /// Test endpoint to generate a 400 Bad Request
        /// </summary>
        [HttpGet("badrequest")]
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiErrorResponse(400));
        }

        /// <summary>
        /// Test endpoint to generate a 400 Bad Request with validation errors
        /// </summary>
        [HttpGet("badrequest/{id}")]
        public ActionResult GetBadRequest(int id)
        {
            return Ok();
        }

        /// <summary>
        /// Test endpoint to generate a 401 Unauthorized
        /// </summary>
        [HttpGet("unauthorized")]
        public ActionResult GetUnauthorized()
        {
            return Unauthorized(new ApiErrorResponse(401));
        }

        /// <summary>
        /// Test endpoint to generate validation errors
        /// Send invalid data (e.g., empty name, negative price, invalid URL) to trigger validation errors
        /// </summary>
        [HttpPost("validationerror")]
        public ActionResult GetValidationError(CreateProductDto product)
        {
            // If we reach here, validation passed
            // To trigger validation errors, send invalid data in the request body:
            // - Empty or missing required fields (Name, Description, etc.)
            // - Price outside range (0.01 - 10000)
            // - Invalid PictureUrl format
            // - QuantityInStock less than 0
            return Ok(new { message = "Validation passed", product });
        }
    }
}