using Application.Helpers;

namespace Application.QueryParams.Level
{
    public class LevelQueryParams : PaginationParams
    {
        public int? LevelID { get; set; }
        public string? LevelName { get; set; }
        public int? MinXPNeeded { get; set; }
        public LevelOrderBy OrderBy { get; set; } = LevelOrderBy.RequiredXPAsc;
    }
}