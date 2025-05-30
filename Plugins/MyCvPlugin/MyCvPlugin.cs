﻿using AiConnector.SemanticKernel.MongoDb;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.DataFormats.Pdf;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Reflection;

namespace MyCvPlugin
{
    public sealed class MyCvPlugin(
        IMemoryClient memoryClient,
        Kernel kernel,
        ILogger<MyCvPlugin> logger)
    {
        private const string FunctionName = "CvProvider";
        private readonly MemoryCollection _memoryCollection = new("embedding", "myCvMemory");
        private readonly string _pathToCv = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!, "Storage/cv.pdf");
        private readonly IPromptTemplateFactory _promptTemplateFactory = new KernelPromptTemplateFactory();
        private readonly Guid _pluginId = Guid.NewGuid();

        public async Task InitializeMyCv()
        {
            LogInformation("The MyCvPlugin is being initialized");

            if (!File.Exists(_pathToCv))
            {
                var errorMessage = $"The cv {_pathToCv} doesn't exist";
                LogError(errorMessage);
                throw new FileNotFoundException(errorMessage);
            }

            try
            {
                if (!await memoryClient.TryInitializeCollection(_memoryCollection.CollectionName, _pathToCv))
                {
#pragma warning disable KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    PdfDecoder decoder = new();
#pragma warning restore KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    var content = await decoder.DecodeAsync(_pathToCv);
                    foreach (var section in content.Sections)
                    {
                        await memoryClient.Save(_memoryCollection, section.Content, $"{_pathToCv}_{Guid.NewGuid()}" /*TODO:*/);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                throw;
            }
        }

        [KernelFunction(FunctionName), Description("Get information about Dmitry Lukyanov's from my cv.")]
        public async Task<string> GetAsync(KernelArguments arguments)
        {
            LogInformation($"{FunctionName} is being called");

            var prompt = @"Provide all information from the cv";

            string[] results;
            try
            {
                var renderedPrompt = await _promptTemplateFactory.Create(new PromptTemplateConfig(template: prompt)).RenderAsync(kernel, arguments);
                results = await memoryClient.Search(_memoryCollection, renderedPrompt);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                throw;
            }

            LogInformation($"{FunctionName} has been successfully called");

            return string.Join('\n', results);
        }

        private void LogInformation(string message) => logger.LogInformation("Plugin Id: {0}. {1}", _pluginId, message);
        private void LogError(string message, Exception? ex = null) => logger.LogError(ex, "Plugin Id: {0}. {1}", _pluginId, message);
    }
}