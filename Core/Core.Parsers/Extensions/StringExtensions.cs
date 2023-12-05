using System.Text.RegularExpressions;
using Core.Models;

namespace Core.Parsers.Extensions;

internal static class StringExtensions
{
    private static readonly Regex _password = new("^(?=.*[0-9].*[0-9])(?=.*[a-z].*[a-z].*[a-z]).{8}$");
    
    public static IEnumerable<Account> ReadAccounts(
        this string filepath,
        Predicate<string>? urlPredicate = null, 
        Predicate<string>? usernamePredicate = null, 
        Predicate<string>? passwordPredicate = null)
    {
        using var reader = new StreamReader(filepath);
        
        while (!reader.EndOfStream)
        {
            var account = reader.ReadAccount(urlPredicate, usernamePredicate, passwordPredicate);
                
            if (account is null) continue;
            
            yield return account;
        }
    }
    
    public static Account? ReadAccount(this StreamReader reader, 
        Predicate<string>? urlPredicate = null, 
        Predicate<string>? usernamePredicate = null, 
        Predicate<string>? passwordPredicate = null)
    {
        var line = reader.ReadLine();
                
        if (line is null || !line.StartsWith("URL")) return null;

        string? url, username, password;
        
        try
        {
            url = line["URL: ".Length..line.Length];
        }
        catch
        {
            return null;
        }

        if (!urlPredicate?.Invoke(url) ?? false) return null;

        username = reader.ReadLine();

        try
        {
            username = username?[(username.IndexOf(' ')+1)..username.Length];
        }
        catch
        {
            return null;
        }
        
        if (username is null || (!usernamePredicate?.Invoke(username) ?? false) || username == "UNKNOWN") return null;
                
        password = reader.ReadLine();

        try
        {
            password = password?[(password.IndexOf(' ')+1)..password.Length];
        }
        catch
        {
            return null;
        }
        
        if (password is null || !_password.IsMatch(password) || (!passwordPredicate?.Invoke(password) ?? false)) return null;

        return new Account(username, password, url);
    }
}