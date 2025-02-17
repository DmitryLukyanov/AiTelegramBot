using AiConnector.SemanticKernel.ChromaDb;
using Microsoft.KernelMemory.DataFormats.Pdf;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace TelegramBot
{
    public sealed class MyCvPlugin(
        IMemoryClient memoryClient,
        Kernel kernel)
    {
        private static readonly MemoryCollection _memoryCollection = new MemoryCollection("myCvMemory");
        private static readonly string _pathToCv = "../Storage/cv.pdf";

        private const string FunctionName = "CvProvider";

        private readonly IPromptTemplateFactory _promptTemplateFactory = new KernelPromptTemplateFactory();

        public async Task InitializeMyCv()
        {
            if (!File.Exists(_pathToCv))
            {
                throw new FileNotFoundException($"The cv {_pathToCv} doesn't exist");
            }

            await memoryClient.CreateCollection(_memoryCollection.CollectionName);
#pragma warning disable KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            PdfDecoder decoder = new();
#pragma warning restore KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var content = await decoder.DecodeAsync(_pathToCv);
            foreach (var section in content.Sections)
            {
                await memoryClient.Save(_memoryCollection, section.Content, $"{_pathToCv}_{Guid.NewGuid()}" /*TODO:*/);
            }
        }

        [KernelFunction(FunctionName), Description("Get information about Dmitry Lukyanov's from my cv.")]
        public async Task<string> GetAsync(KernelArguments arguments)
        {
            var prompt = @"Provide all information from the cv";

            var renderedPrompt = await _promptTemplateFactory.Create(new PromptTemplateConfig(template: prompt)).RenderAsync(kernel, arguments);
            var results = await memoryClient.Search(_memoryCollection, renderedPrompt);
            return string.Join('\n', results);
        }
    }
}