using ITI_Project.Core.Models;
using ITI_Project.Core.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI_Project.Repository
{
    internal static class SpecificationsEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> InputQuery, ISpecifications<TEntity> spec)
        {
            var query = InputQuery;

            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy is not null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending is not null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            if (spec.ThenOrderBy is not null) // Handle ThenBy for additional sorting
            {
                query = ((IOrderedQueryable<TEntity>)query).ThenBy(spec.ThenOrderBy);
            }
            else if (spec.ThenOrderByDescending is not null) // Handle ThenByDescending if needed
            {
                query = ((IOrderedQueryable<TEntity>)query).ThenByDescending(spec.ThenOrderByDescending);
            }

            if (spec.IsPaginationEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));

            query = spec.ThenIncludes.Aggregate(query, (currentQuery, includeExpression) => includeExpression(currentQuery));


            return query;
        }
    }
}
