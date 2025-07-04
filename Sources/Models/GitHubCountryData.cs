/* ----------------------------------------------------------------------- *

    * GitHubCountryData.cs

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

namespace VPNThing.Models;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Represents country data from GitHub.
/// </summary>
public class GitHubCountryData
{
  // -------------------------------------------------------------------------
  public NameData name { get; set; } = new();
  public string cca2 { get; set; } = "";
  public string cca3 { get; set; } = "";

  // -------------------------------------------------------------------------
  public string region { get; set; } = "";
  public string subregion { get; set; } = "";
  public string[] capital { get; set; } = Array.Empty<string>();
  public string[] altSpellings { get; set; } = Array.Empty<string>();

  // -------------------------------------------------------------------------
  public Dictionary<string, CurrencyData> currencies { get; set; } = new();
  public string[] timezones { get; set; } = Array.Empty<string>();
}

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
public class NameData
{
  // -------------------------------------------------------------------------
  public string common { get; set; } = "";
  public string official { get; set; } = "";
  public Dictionary<string, NativeName> nativeName { get; set; } = new();
}

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
public class NativeName
{
  // -------------------------------------------------------------------------
  public string official { get; set; } = "";
  public string common { get; set; } = "";
}

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
public class CurrencyData
{
  // -------------------------------------------------------------------------
  public string name { get; set; } = "";
  public string symbol { get; set; } = "";
}
