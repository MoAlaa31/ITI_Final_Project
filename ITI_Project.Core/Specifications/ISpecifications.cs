using ITI_Project.Core.Models;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ITI_Project.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        Expression<Func<T, bool>> Criteria { get; set; }
        List<Expression<Func<T, object>>> Includes { get; set; }

        Expression<Func<T, object>> OrderBy { get; set; }
        Expression<Func<T, object>> OrderByDescending { get; set; }
        Expression<Func<T, object>> ThenOrderBy { get; set; }
        Expression<Func<T, object>> ThenOrderByDescending { get; set; }

        List<Func<IQueryable<T>, IQueryable<T>>> ThenIncludes { get; set; }
        int Skip { get; set; }
        int Take { get; set; }
        bool IsPaginationEnabled { get; set; }

        LambdaExpression? Selector { get; set; }
    }
}
