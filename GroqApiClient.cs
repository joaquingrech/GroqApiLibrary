using System.Text;
using System.Text.Json;

namespace GroqApiLibrary;

public class GroqApiClient : IGroqApiClient
{
    private readonly HttpClient httpClient = new();

    public GroqApiClient(string apiKey)
    {
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

	public async Task<JsonElement> CreateChatCompletionAsync(JsonElement request)
	{
		// Commented out until stabilized on Groq
		// the API is still not accepting the request payload in its documented format, even after following the JSON mode instructions.
		// var modifiedRequest = request.Clone();
		// modifiedRequest.GetProperty("response_format").Add("type", JsonValue.Create("json_object"));

		var jsonRequestString = JsonSerializer.Serialize(request);
		using var httpContent = new StringContent(jsonRequestString, Encoding.UTF8, "application/json");

		HttpResponseMessage response = await httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", httpContent);
		response.EnsureSuccessStatusCode();
		string responseString = await response.Content.ReadAsStringAsync();

		using var doc = JsonDocument.Parse(responseString);
		return doc.RootElement.Clone();
	}


	public async IAsyncEnumerable<JsonElement> CreateChatCompletionStreamAsync(JsonElement request)
	{
		// Modify the request to add "stream: true" (assuming request is initially mutable or rebuilt as needed)
		var requestObj = JsonSerializer.Deserialize<Dictionary<string, object>>(request.GetRawText());
		requestObj["stream"] = true;
		var jsonRequestString = JsonSerializer.Serialize(requestObj);

		using var httpContent = new StringContent(jsonRequestString, Encoding.UTF8, "application/json");
		HttpResponseMessage response = await httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", httpContent);
		response.EnsureSuccessStatusCode();

		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream)
		{
			var line = await reader.ReadLineAsync();
			if (line.StartsWith("data: "))
			{
				var data = line["data: ".Length..];
				if (data != "[DONE]")
				{
					using var doc = JsonDocument.Parse(data);
					yield return doc.RootElement.Clone();
				}
			}
		}
	}
}