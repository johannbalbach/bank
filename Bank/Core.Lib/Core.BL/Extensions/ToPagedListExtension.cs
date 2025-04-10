using Core.DTO.DTOs.Other;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.Extensions
{
    public static class ToPagedListExtension
    {
        public static async Task<(List<T>, PagedListMetaDataNew)> ToPagedListAsync<T>(this IQueryable<T> query, int page, int size)
        {
            if(page < 1)
            {
                throw new InvalidDataException("Page cannot be less than 1");
            }

            if(size < 1)
            {
                throw new InvalidDataException("Size cannot be less than 1");
            }

            int querySize = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(querySize / (size * 1.0));

            if (page > totalPages)
            {
                page = 1;
            }

            List<T> list = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            return (list, new PagedListMetaDataNew
            {
                PageCount = totalPages,
                TotalItemCount = list.Count,
                PageNumber = page,
                PageSize = size,
                HasPreviousPage = totalPages >= 2 && page > 1,
                HasNextPage = totalPages >= 2 && page < totalPages,
                IsFirstPage = page == 1,
                IsLastPage = page == totalPages || totalPages == 0
            });
        }
    }
}
