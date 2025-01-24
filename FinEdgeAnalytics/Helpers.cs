using System.Text.RegularExpressions;

using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics;

public static class Helpers
{
	public static List<LoaderDto> RemoveDuplicates(this List<LoaderDto> transactions)
	{
		var result = transactions
			.Distinct()
			.ToList();

		return result;
	}

	public static string Pluralize(string input, int number)
	{
		var isPlural = number > 1;
		var endWithVowel = EndsWithVowel(input);
		var suffix = (isPlural, endWithVowel) switch
		{
			(false, _) => "",
			(true, false) => "s",
			(true, true) => "es"
		};

		return $"{input}{suffix}";
	}

	private static bool EndsWithVowel(string str)
	{
		if (string.IsNullOrEmpty(str)) return false;
		var result = Regex.IsMatch(str, "[aeiou]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		return result;
	}
}