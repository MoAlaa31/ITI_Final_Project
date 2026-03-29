namespace ITI_Project.Core.Specifications.CommentSpecs
{
    public class CommentSpecParams
    {
        private const int MaxPageSize = 25;
        public int PageIndex { get; set; } = 1;

        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
    }
}
