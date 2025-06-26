/* ----------------------------------------------------------------------- *

    * LocationLookupService.cs

    ----------------------------------------------------------------------

    Copyright (C) 2025, Turgut Hakkı Özdemir
    All Rights Reserved.

    THIS SOFTWARE IS PROVIDED 'AS-IS', WITHOUT ANY EXPRESS
    OR IMPLIED WARRANTY. IN NO EVENT WILL THE AUTHOR(S) BE HELD LIABLE FOR
    ANY DAMAGES ARISING FROM THE USE OR DISTRIBUTION OF THIS SOFTWARE

    Permission is granted to anyone to use this software for any purpose,
    including commercial applications, and to alter it and redistribute it
    freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not
       claim that you wrote the original software.
    2. Altered source versions must be plainly marked as such, and must not be
       misrepresented as being the original software.
    3. This notice may not be removed or altered from any source distribution.
    4. Distributions in binary form must reproduce the above copyright notice
       and an acknowledgment of use of this software either in documentation
       or binary form itself.

    Turgut Hakkı Özdemir <turgut.hakki@gmail.com>

    Note: This program was written by an AI agent (GitHub Copilot).

* ------------------------------------------------------------------------ */
using System.IO;
using System.Net.Http;
using System.Text.Json;
using VPNThing.Models;

namespace VPNThing.Services;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Provides country and city lookup for VPN servers.
/// </summary>
public class LocationLookupService
{
  // -------------------------------------------------------------------------
  private static readonly HttpClient httpClient = new();
  private Dictionary<string, GitHubCountryData>? githubCountryDatabase;
  private Dictionary<string, CountryInfo>? restCountryDatabase;
  private readonly string cacheFile = Path.Combine(Directory.GetCurrentDirectory(), "Configs", "countries_cache.json");
  private readonly TimeSpan cacheExpiry = TimeSpan.FromDays(7); // Update weekly

  // -------------------------------------------------------------------------
  public async Task<Dictionary<string, GitHubCountryData>> loadGitHubCountryDatabaseAsync()
  {
    if (githubCountryDatabase != null)
      return githubCountryDatabase;

    // Try to load from cache first
    if (await tryLoadFromCacheAsync())
      return githubCountryDatabase!;

    try {
      Console.WriteLine("Loading country database from GitHub (mledoze/countries)...");
      var response = await httpClient.GetStringAsync(
        "https://raw.githubusercontent.com/mledoze/countries/master/countries.json");
      var countries = JsonSerializer.Deserialize<GitHubCountryData[]>(response, new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
      });

      githubCountryDatabase = countries?.ToDictionary(c => c.cca2.ToLower(), c => c) ?? new();
      Console.WriteLine($"Loaded {githubCountryDatabase.Count} countries from GitHub");

      // Cache the data
      await saveToCacheAsync();

      return githubCountryDatabase;
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to load from GitHub: {ex.Message}");
      Console.WriteLine("Falling back to REST Countries API...");
      return await loadRESTCountryDatabaseFallbackAsync();
    }
  }

  // -------------------------------------------------------------------------
  private async Task<bool> tryLoadFromCacheAsync()
  {
    try {
      if (!File.Exists(cacheFile))
        return false;

      var fileInfo = new FileInfo(cacheFile);
      if (DateTime.Now - fileInfo.LastWriteTime > cacheExpiry) {
        Console.WriteLine("Cache expired, will fetch fresh data...");
        return false;
      }

      Console.WriteLine("Loading country database from cache...");
      var json = await File.ReadAllTextAsync(cacheFile);
      var countries = JsonSerializer.Deserialize<GitHubCountryData[]>(json, new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
      });

      githubCountryDatabase = countries?.ToDictionary(c => c.cca2.ToLower(), c => c) ?? new();
      Console.WriteLine($"Loaded {githubCountryDatabase.Count} countries from cache");
      return true;
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to load from cache: {ex.Message}");
      return false;
    }
  }

  // -------------------------------------------------------------------------
  private async Task saveToCacheAsync()
  {
    try {
      if (githubCountryDatabase == null)
        return;

      var countries = githubCountryDatabase.Values.ToArray();
      var json = JsonSerializer.Serialize(countries, new JsonSerializerOptions {
        WriteIndented = false
      });
      await File.WriteAllTextAsync(cacheFile, json);
      Console.WriteLine("Country data cached successfully");
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to cache country data: {ex.Message}");
    }
  }

  // -------------------------------------------------------------------------
  private async Task<Dictionary<string, GitHubCountryData>> loadRESTCountryDatabaseFallbackAsync()
  {
    try {
      Console.WriteLine("Loading from REST Countries API as fallback...");
      var response = await httpClient.GetStringAsync(
        "https://restcountries.com/v3.1/all?fields=name,cca2,cca3,region,subregion,capital");
      var countries = JsonSerializer.Deserialize<CountryInfo[]>(response, new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
      });

      restCountryDatabase = countries?.ToDictionary(c => c.cca2.ToLower(), c => c) ?? new();

      // Convert to GitHub format for consistency
      githubCountryDatabase = restCountryDatabase.ToDictionary(
        kvp => kvp.Key, kvp => new GitHubCountryData {
          cca2 = kvp.Value.cca2,
          cca3 = kvp.Value.cca3,
          name = new NameData {
            common = kvp.Value.name.common,
            official = kvp.Value.name.official
          },
          region = kvp.Value.region,
          subregion = kvp.Value.subregion,
          capital = kvp.Value.capital
        });

      Console.WriteLine($"Loaded {githubCountryDatabase.Count} countries from REST Countries API");
      return githubCountryDatabase;
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to load from REST Countries API: {ex.Message}");
      Console.WriteLine("Using built-in country mapping...");
      githubCountryDatabase = new Dictionary<string, GitHubCountryData>();
      return githubCountryDatabase;
    }
  }

  // -------------------------------------------------------------------------
  public async Task<string> getCountryNameAsync(string countryCode)
  {
    var database = await loadGitHubCountryDatabaseAsync();
    if (database.TryGetValue(countryCode.ToLower(), out var country)) {
      return country.name.common;
    }

    // Fallback to built-in mapping if all online sources fail
    return getFallbackCountryName(countryCode);
  }

  // -------------------------------------------------------------------------
  public async Task forceUpdateDatabaseAsync()
  {
    Console.WriteLine("Forcing database update...");    // Delete cache file to force refresh
    if (File.Exists(cacheFile)) {
      File.Delete(cacheFile);
    }

    // Clear in-memory cache
    githubCountryDatabase = null;
    restCountryDatabase = null;

    // Reload fresh data
    await loadGitHubCountryDatabaseAsync();
  }

  // -------------------------------------------------------------------------
  private string getFallbackCountryName(string countryCode)
  {
    var fallbackMap = new Dictionary<string, string> {
      ["al"] = "Albania", ["at"] = "Austria", ["au"] = "Australia", ["be"] = "Belgium",
      ["bg"] = "Bulgaria", ["br"] = "Brazil", ["ca"] = "Canada", ["ch"] = "Switzerland",
      ["cl"] = "Chile", ["co"] = "Colombia", ["cy"] = "Cyprus", ["cz"] = "Czech Republic",
      ["de"] = "Germany", ["dk"] = "Denmark", ["ee"] = "Estonia", ["es"] = "Spain",
      ["fi"] = "Finland", ["fr"] = "France", ["gb"] = "United Kingdom", ["gr"] = "Greece",
      ["hk"] = "Hong Kong", ["hr"] = "Croatia", ["hu"] = "Hungary", ["id"] = "Indonesia",
      ["ie"] = "Ireland", ["il"] = "Israel", ["it"] = "Italy", ["jp"] = "Japan",
      ["mx"] = "Mexico", ["my"] = "Malaysia", ["ng"] = "Nigeria", ["nl"] = "Netherlands",
      ["no"] = "Norway", ["nz"] = "New Zealand", ["pe"] = "Peru", ["ph"] = "Philippines",
      ["pl"] = "Poland", ["pt"] = "Portugal", ["ro"] = "Romania", ["rs"] = "Serbia",
      ["se"] = "Sweden", ["sg"] = "Singapore", ["si"] = "Slovenia", ["sk"] = "Slovakia",
      ["th"] = "Thailand", ["tr"] = "Turkey", ["ua"] = "Ukraine", ["us"] = "United States",
      ["za"] = "South Africa"
    };

    return fallbackMap.TryGetValue(countryCode.ToLower(), out var name) ? name : countryCode.ToUpper();
  }
}
