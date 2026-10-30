using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

// Smoke HTTP em C#, sem pacotes externos. Retorna 0 em sucesso e 1 em falha.
// --validate-config verifica os secrets por variáveis, sem exibi-los.
try
{
    if (args.Length == 1 && args[0] == "--validate-config")
    {
        ValidateConfiguration();
        Console.WriteLine("Pré-validação de configuração: OK (segredos omitidos)");
        return 0;
    }

    if (args.Length < 1 || !Uri.TryCreate(args[0], UriKind.Absolute, out var baseUri)
        || (baseUri.Scheme != "http" && baseUri.Scheme != "https"))
        throw new InvalidOperationException("Informe a URL HTTP/HTTPS do ambiente.");

    var options = new Dictionary<string, string>();
    for (int i = 1; i < args.Length; i += 2)
    {
        if (i + 1 >= args.Length || args[i] is not ("--expected-environment" or "--expected-revision" or "--output"))
            throw new InvalidOperationException("Opções inválidas para o smoke.");
        options.Add(args[i], args[i + 1]);
    }

    var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    if (string.IsNullOrWhiteSpace(password))
        throw new InvalidOperationException("Defina ADMIN_PASSWORD no ambiente, sem colocá-la na linha de comando.");

    using var client = new HttpClient { BaseAddress = baseUri, Timeout = TimeSpan.FromSeconds(10) };
    string? environment = null;
    string? revision = null;
    bool ready = false;
    for (int attempt = 0; attempt < 30; attempt++)
    {
        try
        {
            using var response = await client.GetAsync("/healthz");
            if (response.IsSuccessStatusCode)
            {
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (body.RootElement.GetProperty("status").GetString() == "healthy")
                {
                    environment = body.RootElement.GetProperty("environment").GetString();
                    revision = body.RootElement.GetProperty("revision").GetString();
                    ready = true;
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException) { }
        if (attempt < 29) await Task.Delay(TimeSpan.FromSeconds(2));
    }
    Require(ready, "Readiness não ficou saudável dentro do prazo.");
    if (options.TryGetValue("--expected-environment", out var expectedEnvironment))
        Require(environment == expectedEnvironment, "Ambiente retornado não corresponde ao deploy.");
    if (options.TryGetValue("--expected-revision", out var expectedRevision))
        Require(revision == expectedRevision, "Revisão retornada não corresponde à imagem esperada.");

    Require((await client.GetStringAsync("/")).Contains("HydroGuard"), "Frontend ausente.");
    using (var swagger = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json")))
        Require(swagger.RootElement.GetProperty("paths").EnumerateObject().Any(), "Swagger sem rotas.");
    using (var anonymous = await client.GetAsync("/api/medidores"))
        Require(anonymous.StatusCode == HttpStatusCode.Unauthorized, "Rota protegida não retornou 401.");
    using var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password });
    Require(login.IsSuccessStatusCode, "Login não retornou sucesso.");
    using var loginBody = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
    var token = loginBody.RootElement.GetProperty("token").GetString();
    Require(!string.IsNullOrWhiteSpace(token), "Login não retornou token.");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    using (var medidores = JsonDocument.Parse(await client.GetStringAsync("/api/medidores?page=1&pageSize=10")))
        Require(medidores.RootElement.TryGetProperty("items", out _), "Listagem de medidores inválida.");
    using (var esg = JsonDocument.Parse(await client.GetStringAsync("/api/esg/indicadores")))
        Require(esg.RootElement.TryGetProperty("scoreESG", out _), "Indicadores ESG inválidos.");

    var result = new
    {
        status = "passed",
        checked_at_utc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
        base_url = baseUri.ToString(),
        environment,
        revision,
        checks = new[] { "database_readiness", "frontend", "swagger", "unauthorized_401", "jwt_login", "medidores", "esg" }
    };
    var output = options.GetValueOrDefault("--output", "evidence/smoke.json");
    var outputPath = Path.GetFullPath(output);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(outputPath, json + Environment.NewLine);
    Console.WriteLine(json);
    return 0;
}
catch (InvalidOperationException ex)
{
    // Apenas mensagens definidas neste programa. Não imprime corpos HTTP/segredos.
    Console.Error.WriteLine("Smoke/configuração falhou: " + ex.Message);
    return 1;
}
catch (Exception)
{
    Console.Error.WriteLine("Smoke/configuração falhou: erro de conexão, resposta inválida ou arquivo de saída.");
    return 1;
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void ValidateConfiguration()
{
    foreach (var key in new[] { "MSSQL_SA_PASSWORD", "ADMIN_PASSWORD" })
    {
        var value = Environment.GetEnvironmentVariable(key) ?? "";
        Require(value.Length >= 12 && !value.Contains("SUBSTITUA"), key + ": forneça senha real com 12+ caracteres.");
        Require(Regex.IsMatch(value, @"\A[A-Za-z0-9!@#%_+=.\-]+\z"), key + ": use letras, números e !@#%_+=.-, sem espaços, ; ou $.");
    }
    var sql = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD")!;
    Require(Regex.IsMatch(sql, "[A-Z]") && Regex.IsMatch(sql, "[a-z]") && Regex.IsMatch(sql, "[0-9]")
        && Regex.IsMatch(sql, @"[!@#%_+=.\-]"), "MSSQL_SA_PASSWORD: inclua maiúscula, minúscula, número e símbolo.");
    var jwt = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";
    Require(Encoding.UTF8.GetByteCount(jwt) >= 32 && !jwt.Contains("SUBSTITUA"), "JWT_KEY: forneça chave real com 32+ bytes.");
    var environment = Environment.GetEnvironmentVariable("APP_ENVIRONMENT");
    Require(environment is "Development" or "Staging" or "Production", "APP_ENVIRONMENT inválido.");
    Require(environment != "Production" || string.Equals(Environment.GetEnvironmentVariable("SEED_DEMO_DATA") ?? "false", "false", StringComparison.OrdinalIgnoreCase),
        "Production não permite dados demonstrativos.");
    Require(int.TryParse(Environment.GetEnvironmentVariable("API_PORT") ?? "5000", out var port) && port is >= 1 and <= 65535, "API_PORT inválida.");
}
