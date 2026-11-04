using System.Text.Json.Nodes;
using Json.Schema;

namespace HydroGuard.Tests;

internal static class ContractAssert
{
  public static async Task<JsonNode> ReadAsync(
      HttpResponseMessage response, HttpStatusCode status, string contract)
  {
    Assert.Equal(status, response.StatusCode);
    Assert.Contains(response.Content.Headers.ContentType?.MediaType,
        new[] { "application/json", "application/problem+json" });

    var body = JsonNode.Parse(await response.Content.ReadAsStringAsync());
    Assert.NotNull(body);
    var path = Path.Combine(AppContext.BaseDirectory, "Schemas", contract + ".schema.json");
    var schema = JsonSchema.FromText(await File.ReadAllTextAsync(path));
    var result = schema.Evaluate(body, new EvaluationOptions
    {
      RequireFormatValidation = true,
      OutputFormat = OutputFormat.List
    });
    Assert.True(result.IsValid, $"Contrato {contract} inválido: {System.Text.Json.JsonSerializer.Serialize(result)}");
    return body;
  }
}
