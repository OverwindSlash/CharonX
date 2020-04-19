using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Extensions;
using Abp.Linq.Extensions;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace CharonX
{
    public class PagingHelper
    {
        public static IQueryable<TEntity> ApplySorting<TEntity, TPrimaryKey>(IQueryable<TEntity> query, PagedResultRequestDto input)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            //Try to sort query if available
            var sortInput = input as ISortedResultRequest;
            if (sortInput != null)
            {
                if (!sortInput.Sorting.IsNullOrWhiteSpace())
                {
                    return query.OrderBy(sortInput.Sorting);
                }
            }

            //IQueryable.Task requires sorting, so we should sort if Take will be used.
            if (input is ILimitedResultRequest)
            {
                return query.OrderBy(e => e.Id);
            }

            //No sorting
            return query;
        }

        public static IQueryable<TEntity> ApplyPaging<TEntity, TPrimaryKey>(IQueryable<TEntity> query, PagedResultRequestDto input)
            where TEntity : class, IEntity<TPrimaryKey>
        {
            //Try to use paging if available
            var pagedInput = input as IPagedResultRequest;
            if (pagedInput != null)
            {
                return query.PageBy(pagedInput);
            }

            //Try to limit query result if available
            var limitedInput = input as ILimitedResultRequest;
            if (limitedInput != null)
            {
                return query.Take(limitedInput.MaxResultCount);
            }

            //No paging
            return query;
        }
    }
}
