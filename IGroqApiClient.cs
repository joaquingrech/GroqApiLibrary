using System.Text.Json;
using System.Threading.Tasks;

namespace GroqApiLibrary
{
    public interface IGroqApiClient
    {
        Task<JsonElement> CreateChatCompletionAsync(JsonElement request);

        IAsyncEnumerable<JsonElement> CreateChatCompletionStreamAsync(JsonElement request);
    }
}