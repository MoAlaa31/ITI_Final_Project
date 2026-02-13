using ITI_Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ITI_Project.Core.Specifications
{
    public class BaseSpecifications<T> : ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new List<Expression<Func<T, object>>>();
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        public Expression<Func<T, object>> ThenOrderBy { get; set; }
        public Expression<Func<T, object>> ThenOrderByDescending { get; set; }
        public List<Func<IQueryable<T>, IQueryable<T>>> ThenIncludes { get; set; } = new List<Func<IQueryable<T>, IQueryable<T>>>();

        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsPaginationEnabled { get; set; }
        public Expression<Func<T, object>> Selector { get; set; }

        public BaseSpecifications()
        {

        }

        public BaseSpecifications(Expression<Func<T, bool>> CriteriaExpression)
        {
            Criteria = CriteriaExpression;
        }

        public void AddOrderBy(Expression<Func<T, object>> OrderByExpression)
        {
            OrderBy = OrderByExpression;
        }
        public void AddOrderByDescending(Expression<Func<T, object>> OrderByExpressionDescending)
        {
            OrderByDescending = OrderByExpressionDescending;
        }

        public void AddThenOrderBy(Expression<Func<T, object>> ThenOrderByExpression)
        {
            ThenOrderBy = ThenOrderByExpression;
        }
        public void AddThenOrderByDescending(Expression<Func<T, object>> ThenOrderByExpressionDescending)
        {
            ThenOrderByDescending = ThenOrderByExpressionDescending;
        }

        public void ApplyPagination(int skip, int take)
        {
            IsPaginationEnabled = true;
            Skip = skip;
            Take = take;
        }

        public void ApplySelector(Expression<Func<T, object>> selectorExpression)
        {
            Selector = selectorExpression;
        }
    }
}
