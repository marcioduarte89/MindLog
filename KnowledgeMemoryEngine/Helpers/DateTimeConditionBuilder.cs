using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using MindLog.Shared.Models;
using System.Globalization;
using WeaviateNET.Query.ConditionalOperator;

namespace KnowledgeBaseEngine.Helpers
{
    public static class DateTimeConditionBuilder
    {
        public static Conditional<StorageModelWeaviate>? GetDateTimeFilter(string query)
        {
            var resultText = DateTimeRecognizer.RecognizeDateTime(query, Culture.English);

            if (resultText is null || !resultText.Any())
            {
                return null;
            }
            
            // for simplicity sake we only support 1 date
            if(resultText.Count > 1)
            {
                throw new Exception("Currenly we don't support more than 1 date source in the query");   
            }

            SortedDictionary<string, object> values = resultText.Single().Resolution;
            var valuesList = (IList<Dictionary<string, string>>)values["values"];

            if (valuesList.Count > 1)
            {
                throw new Exception("Currenly we don't support more than 1 date source in the query");
            }

            var dateValues = valuesList.Single();

            if (dateValues["type"].Equals("daterange", StringComparison.OrdinalIgnoreCase))
            {
                var start = DateTime.Parse(dateValues["start"]).ToUniversalTime();
                var end = DateTime.Parse(dateValues["end"]).ToUniversalTime();

                return Conditional<StorageModelWeaviate>.And(
                    When<StorageModelWeaviate, DateTime>.GreaterThanEqual("noteDate", start),
                    When<StorageModelWeaviate, DateTime>.LessThanEqual("noteDate", end)
                );
            }
            else if (dateValues["type"].Equals("date", StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {
                throw new Exception("Date format provided is not supported");
            }

            return null;
        }
    }
}
