//--------------------------------------------------------------------------------------------------------------------
// <copyright company="Sitecore Corporation" file="StringExtensions.cs">
//   Copyright (C) 2016 by Sitecore Corporation
// </copyright>
// <summary>
//   Represents a set of extension methods for Sitecore Query processing.
// </summary>
// -------------------------------------------------------------------------------------------------------------------- 
namespace Sitecore.Support.Extensions.SitecoreQueryExtensions
{
  using System;
  using Sitecore.Diagnostics;

  /// <summary>
  ///   A set of extension methods for <see cref="Sitecore.Data.Query"/> namespace.
  /// </summary>
  public static class StringExtensions
  {
    #region Fields

    /// <summary>
    ///   The Sitecore Fast query prefix.
    /// <para>Value: 'fast:'</para>
    /// </summary>
    public static readonly string FastQueryPrefix = @"fast:";

    /// <summary>
    ///   The Sitecore query prefix.
    ///   <para>Value: 'query:'</para>
    /// </summary>
    public static readonly string QueryPrefix = @"query:";

    #endregion

    #region Public methods

    /// <summary>
    ///   Escapes the query path.
    /// </summary>
    /// <param name="queryPath">The query path.</param>
    /// <returns>Escaped query path.</returns>
    [NotNull]
    public static string EscapeQueryPath([NotNull] this string queryPath)
    {
      Assert.ArgumentNotNull(queryPath, "queryPath");

      string[] names = queryPath.Split('/');
      bool escaped = false;

      for (int n = 0; n < names.Length; n++)
      {
        string name = names[n];

        if ((name.IndexOf(' ') >= 0 || name.IndexOf('-') >= 0) && !(name.StartsWith("#", StringComparison.InvariantCulture) && name.EndsWith("#", StringComparison.InvariantCulture)))
        {
          names[n] = '#' + name + '#';
          escaped = true;
        }
      }

      if (!escaped)
      {
        return queryPath;
      }

      string result = string.Empty;

      int first = string.IsNullOrEmpty(names[0]) ? 1 : 0;

      for (int n = first; n < names.Length; n++)
      {
        result += '/' + names[n];
      }

      return result;
    }

    /// <summary>
    ///   Checks if a <paramref name="value" /> starts the with Sitecore <see cref="FastQueryPrefix" />.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>false</c> if input is <c>null</c>, or does not start with <see cref="FastQueryPrefix" />;<c>true</c>
    ///   otherwise.
    /// </returns>
    public static bool StartsWithFastQueryPrefix([CanBeNull] this string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return false;
      }

      return value.StartsWith(FastQueryPrefix, StringComparison.InvariantCulture);
    }

    /// <summary>
    ///   Checks if a <paramref name="source" /> starts the with Sitecore <see cref="QueryPrefix" />.
    /// <para>Returns <c>false</c> if <paramref name="source"/> is <c>null</c>, or does not start with query prefix.</para>
    /// </summary>
    /// <param name="source">The value.</param>
    /// <returns><c>false</c> if input is <c>null</c>, or does not start with <see cref="QueryPrefix" />;<c>true</c> otherwise.</returns>
    public static bool StartsWithSitecoreQueryPrefix([CanBeNull] this string source)
    {
      if (string.IsNullOrEmpty(source))
      {
        return false;
      }

      return source.StartsWith(QueryPrefix, StringComparison.InvariantCulture);
    }

    /// <summary>
    /// Extracts Sitecore query body from <paramref name="value" /> starting from <see cref="QueryPrefix"/> length in case value starts with query prefix.
    /// <para>
    /// <c>false</c> if <paramref name="value" /> is <c>null</c>, or does not start with
    /// <see cref="QueryPrefix" />; <c>true</c> and substring part otherwise.
    /// </para>
    /// </summary>
    /// <param name="value">The value to be parsed.</param>
    /// <param name="queryBody">The resulting extracted Sitecore Query body.</param>
    /// <returns>
    /// <c>false</c> if <paramref name="value" /> is <c>null</c>, or does not start with
    /// <see cref="QueryPrefix" />; <c>true</c> and substring part otherwise.
    /// </returns>
    [NotNull]
    public static bool TrySubstringSitecoreQueryBody([CanBeNull] this string value, out string queryBody)
    {
      queryBody = string.Empty;
      if (string.IsNullOrEmpty(value) || !StartsWithSitecoreQueryPrefix(value))
      {
        return false;
      }

      queryBody = value.Substring(QueryPrefix.Length);      
      return true;
    }

    #endregion
  }
}