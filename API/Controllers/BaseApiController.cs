using Microsoft.AspNetCore.Mvc;
using Core.Helpers;
using Core.Interfaces;
using Core.Specifications;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        protected async Task<ActionResult<Pagination<T>>> CreatePageResult<T>(
            IGenericRepository<T> repository,
            ISpecification<T> spec,
            ISpecification<T> countSpec,
            int pageIndex,
            int pageSize) where T : class
        {
            // Sequential: DbContext is not thread-safe; list and count must not run concurrently on the same instance.
            var items = await repository.GetAllAsync(spec);
            var totalItems = await repository.CountAsync(countSpec);

            var pagination = new Pagination<T>(
                pageIndex,
                pageSize,
                totalItems,
                items
            );

            return Ok(pagination);
        }
    }
}