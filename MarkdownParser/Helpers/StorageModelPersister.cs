using MindLog.Shared.Models;
using System.Globalization;
using WeaviateNET;

namespace MarkdownParser.Helpers
{
    public static class StorageModelPersister
    {
        public static async Task Save(IEnumerable<StorageModel> storageModel)
        {
            var weaviateDB = new WeaviateDB("http://localhost:8080/v1");

            await weaviateDB.Schema.Update();

            var mc = weaviateDB.Schema.GetClass<StorageModelWeaviate>("StorageModel") ?? 
                     await weaviateDB.Schema.NewClass<StorageModelWeaviate>("StorageModel");

            var weaviateObject = new List<WeaviateObject<StorageModelWeaviate>>();
            foreach (var model in storageModel)
            {
                var document = mc.Create(); // create the document
                document.Properties.noteDate = model.NoteDate.ToUniversalTime();
                document.Properties.noteName = model.NoteName;
                document.Properties.section = model.Section;
                document.Properties.tags = [.. model.Tags];
                document.Properties.text = model.Text;
                document.Vector = [.. model.Embeddings];
                
                weaviateObject.Add(document);
            }

            await mc.Add(weaviateObject);

            //await mc.Update();

            //var q1 = mc.CreateAggregateQuery();
            //var q = mc.CreateGetQuery(true);
            //var query = new GraphQLQuery()
            //{
            //    Query = q.ToString()
            //};
            //var ret = await weaviateDB.Schema.RawQuery(query);
            //var d = ret.Data["Get"];
            //var data = d["StorageModel"];
            //var a = data.ToObject<StorageModelWeaviate[]>();
        }
    }
}
