using ITI_Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ITI_Project.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; }


        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        public Expression<Func<T, object>> ThenOrderBy { get; set; }
        public Expression<Func<T, object>> ThenOrderByDescending { get; set; }

        public List<Func<IQueryable<T>, IQueryable<T>>> ThenIncludes { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsPaginationEnabled { get; set; }
        public Expression<Func<T, object>> Selector { get; set; }
    }
}
