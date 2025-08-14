namespace KnowledgeBaseEngine.Models
{
    public class TranslatedQuery
    {
        public string Query { get; set; }

        public string[]? Keywords { get; set; }

        public DateRange? DateRange { get; set; }

        public DateTime? Date { get; set; }

        public bool HasDateRange()
        {
            return DateRange is not null;
        }

        public bool HasDate()
        {
            return Date is not null;
        }

        public bool HasKeywords()
        {
            return Keywords is not null || Keywords.Any();
        }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
