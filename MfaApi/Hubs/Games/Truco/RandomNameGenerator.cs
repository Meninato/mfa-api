namespace MfaApi.Hubs.Games.Truco;

public static class RandomNameGenerator
{
    private static readonly string[] adjectives = { "adorable", "amazing", "brave", "charming", "clever", "dashing", "dazzling", "elegant", "fierce", "friendly", "funny", "gentle", "glorious", "handsome", "happy", "helpful", "jolly", "kind", "lively", "lovely", "loyal", "nice", "perfect", "polite", "powerful", "proud", "silly", "talented", "thoughtful", "trustworthy", "wise" };
    private static readonly string[] nouns = { "ant", "bird", "cat", "chicken", "cow", "dog", "dolphin", "duck", "elephant", "fish", "giraffe", "goat", "hamster", "horse", "kangaroo", "lion", "monkey", "otter", "panda", "pig", "rabbit", "snake", "tiger", "turtle", "wolf" };

    public static string GenerateRandomName()
    {
        var adjective = adjectives[Random.Shared.Next(adjectives.Length)];
        var noun = nouns[Random.Shared.Next(nouns.Length)];
        var id = Guid.NewGuid().ToString()[0..4];

        return $"{adjective}_{noun}_{id}";
    }
}