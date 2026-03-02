using ITI_Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ITI_Project.Core.Specifications
{
    public class BaseSpecifications<T> : ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new();
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        public Expression<Func<T, object>> ThenOrderBy { get; set; }
        public Expression<Func<T, object>> ThenOrderByDescending { get; set; }
        public List<Func<IQueryable<T>, IQueryable<T>>> ThenIncludes { get; set; } = new();

        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsPaginationEnabled { get; set; }
        public LambdaExpression? Selector { get; set; }

        public BaseSpecifications()
        {
        }

        public BaseSpecifications(Expression<Func<T, bool>> criteriaExpression)
        {
            Criteria = criteriaExpression;
        }

        public void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        public void AddOrderByDescending(Expression<Func<T, object>> orderByExpressionDescending)
        {
            OrderByDescending = orderByExpressionDescending;
        }

        public void AddThenOrderBy(Expression<Func<T, object>> thenOrderByExpression)
        {
            ThenOrderBy = thenOrderByExpression;
        }

        public void AddThenOrderByDescending(Expression<Func<T, object>> thenOrderByExpressionDescending)
        {
            ThenOrderByDescending = thenOrderByExpressionDescending;
        }

        public void ApplyPagination(int skip, int take)
        {
            IsPaginationEnabled = true;
            Skip = skip;
            Take = take;
        }

        public void ApplySelector<TResult>(Expression<Func<T, TResult>> selectorExpression)
        {
            Selector = selectorExpression;
        }
    }
}
