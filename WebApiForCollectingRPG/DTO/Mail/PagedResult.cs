using System;
using System.Collections.Generic;

namespace WebApiForCollectingRPG.DTO.Mail
{
    public class PagedResult<T>
    {
        public Int32 Page { get; set; }
        public Int32 PerPage { get; set; }
        public Int64 TotalItems { get; set; }
        public List<T> Items { get; set; }
    }
}