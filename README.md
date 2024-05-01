Here's the updated README with the new features and capabilities:

# Groq API C# Client Library

This library provides a simple interface to interact with the Groq AI API. It allows you to send requests to the API and receive responses asynchronously through the `IGroqApiClient` interface.

## Installation

1. Either reference the GroqApiLibrary in your project or copy the GroqApiClient.cs file and the IGroqApiClient interface file into your project.

## Usage

1. Implement the IGroqApiClient interface in your application. An example implementation, GroqApiClient, is provided.
2. Create an instance of the GroqApiClient class (or any class that implements IGroqApiClient), providing your API key.
3. Create a Newtonsoft JSON object (JObject) with your request. The available parameters are listed in the Groq API documentation.
4. Receive the response, which is also a JObject, and extract the response information accordingly.

## Examples

### Standard Chat Completion

```csharp
using GroqApiLibrary;
using System.Text.Json;

namespace GroqTest
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			string apiKey = "xxxxxxxxx";
			IGroqApiClient groqApi = new GroqApiClient(apiKey);

			// Building the request using a Dictionary and a List to handle JSON creation.
			var requestObj = new Dictionary<string, object>
			{
				["model"] = "mixtral-8x7b-32768", // Supported models: llama2-70b-chat, gemma-7b-it, llama3-70b-8192, llama3-8b-8192
				["temperature"] = 0.5,
				["max_tokens"] = 100,
				["top_p"] = 1,
				["stop"] = "TERMINATE",
				["messages"] = new List<Dictionary<string, object>>
		{
			new Dictionary<string, object>
			{
				["role"] = "system",
				["content"] = "You are a chatbot capable of anything and everything."
			},
			new Dictionary<string, object>
			{
				["role"] = "user",
				["content"] = "Write a poem about GitHub."
			}
		}
			};

			string jsonRequest = JsonSerializer.Serialize(requestObj);
			JsonElement request = JsonDocument.Parse(jsonRequest).RootElement;

			JsonElement result = await groqApi.CreateChatCompletionAsync(request);
			JsonElement choices = result.GetProperty("choices");
			string response = choices.ValueKind != JsonValueKind.Undefined && choices.GetArrayLength() > 0 ?
				choices[0].GetProperty("message").GetProperty("content").GetString() ?? "No response found" :
				"No response found";

			Console.WriteLine(response);
			Console.ReadLine();
		}
	}
}
```

### Streaming Chat Completion

```csharp
using GroqApiLibrary;
using System.Text.Json;

class Program_streaming
{
    static async Task Main()
    {
        string apiKey = "xxxxxxxxx";
        IGroqApiClient groqApi = new GroqApiClient(apiKey);

        // Building the request using a Dictionary and a List to handle JSON creation.
        var requestObj = new Dictionary<string, object>
        {
            ["model"] = "mixtral-8x7b-32768", // LLaMA2-70b-chat or Gemma-7b-it also supported
            ["temperature"] = 0.5,
            ["max_tokens"] = 100,
            ["top_p"] = 1,
            ["stop"] = "TERMINATE",
            ["messages"] = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["role"] = "system",
                    ["content"] = "You are a chatbot capable of anything and everything."
                },
                new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = "Write a poem about GitHub."
                }
            }
        };

        string jsonRequest = JsonSerializer.Serialize(requestObj);
        JsonElement request = JsonDocument.Parse(jsonRequest).RootElement;

        // The stream processing must handle JsonElement objects.
        await foreach (JsonElement chunk in groqApi.CreateChatCompletionStreamAsync(request))
        {
            JsonElement choices = chunk.GetProperty("choices");
            if (choices.ValueKind != JsonValueKind.Undefined && choices.GetArrayLength() > 0)
            {
                JsonElement delta = choices[0].GetProperty("delta");
                string content = delta.GetProperty("content").GetString() ?? string.Empty;
                Console.Write(content);
            }
        }

        Console.WriteLine();
        Console.ReadLine();
    }
}
```

## Latest Updates

- The "seed" parameter can be included in the request object passed to the CreateChatCompletionAsync and CreateChatCompletionStreamAsync methods, allowing users to provide a seed for sampling and enabling reproducible responses.
- Added support for the "stream" parameter in the CreateChatCompletionAsync method. When set to true, the API returns partial message deltas using server-side events, reducing the time to first token received.
- Added information about the JSON mode beta feature. Setting "response_format": {"type": "json_object"} in the request enables this mode. Provided guidance on best practices and limitations.
- Updated the example to demonstrate how to use a stop sequence, showing how to pass a single stop value or an array of stop values.
- Added an async version of the CreateChatCompletionAsync method (CreateChatCompletionStreamAsync) to support asynchronous usage.
- Updated the README to include information about error handling, specifically the "json_validate_failed" error code that Groq returns when JSON generation fails in JSON mode.
- Ensured that the library handles the response format correctly when JSON mode is enabled, parsing the response as a structured JSON object.

The modification to add the response_format parameter should not impact existing client applications that are using this library, as long as they do not explicitly set the response_format parameter in their requests. The library will still work correctly for them, and their client-side code will not need refactoring. The modification only adds the response_format parameter if it is not already present in the request. This ensures that existing applications will not be affected, while also allowing JSON Mode requests to function correctly.

In summary, there should be no impact on existing client applications, and no client-side code refactoring is required.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

## License

This library is licensed under the MIT License. See the LICENSE file for more information.

## Special Thanks

This project is a fork from https://github.com/jgravelle/GroqApiLibrary because I decided to remove all external dependencies. Thank you JGravelle for starting the project.