using Azure.Data.Tables;

var connectionString = args.Length > 0 ? args[0] : "UseDevelopmentStorage=true";
var username = args.Length > 1 ? args[1] : "admin";
var password = args.Length > 2 ? args[2] : "admin123";

var client = new TableServiceClient(connectionString);
var table = client.GetTableClient("Users");
await table.CreateIfNotExistsAsync();

var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

var entity = new TableEntity("moderator", username.ToLower())
{
    { "PasswordHash", hash },
    { "CreatedAt", DateTimeOffset.UtcNow }
};

await table.UpsertEntityAsync(entity);
Console.WriteLine($"Moderator '{username}' seeded with password '{password}'.");
