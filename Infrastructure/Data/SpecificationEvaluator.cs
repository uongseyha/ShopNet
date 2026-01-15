using Core.Entities;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }

            if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            return query;
        }

        public static IQueryable<TResult> GetQuery<TResult>(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery.AsQueryable();

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.Select != null)
            {
                var selectQuery = query.Select(spec.Select);

                if (spec.IsDistinct)
                {
                    selectQuery = selectQuery.Distinct();
                }

                if (spec.OrderBy != null)
                {
                    selectQuery = selectQuery.OrderBy(_ => _);
                }

                return selectQuery.Cast<TResult>();
            }

            return query.Cast<TResult>();
        }
    }
}