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
                document.Properties.noteDate = model.NoteDate;
                document.Properties.noteName = model.NoteName;
                document.Properties.section = model.Section;
                document.Properties.tags = [.. model.Tags];
                document.Properties.text = model.Text;
                document.Vector = [.. model.Embeddings];
                
                weaviateObject.Add(document);
            }

            var result = await mc.Add(weaviateObject);
            if (result != null || !result.Any())
            {
                Console.WriteLine("There must have been a problem as the return collection was empty");
            }
        }
    }
}
