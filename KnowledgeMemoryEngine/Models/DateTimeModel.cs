namespace KnowledgeBaseEngine.Models
{
    internal class DateTimeModel
    {
        public DateTime? SpecificDateTime { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public Type DateTimeType { get; set; }
    }

    public enum Type
    {
        Date,
        DateRange
    }
}
