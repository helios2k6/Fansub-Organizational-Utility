using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileNameParser
{
	/// <summary>
	/// A static factory class that parses file names (without the full path) and returns a <see cref="FansubFile"/>
	/// </summary>
	public static class FansubFileParsers
	{
		#region bracket parsers
		private static readonly Parser<string> SquareBracketEnclosedText =
			(from openBracket in BaseGrammars.OpenSquareBracket
			 from content in Parse.CharExcept(']').Many().Text()
			 from closedBracket in BaseGrammars.ClosedSquareBracket
			 select content).Token();

		private static readonly Parser<string> SquareBracketEnclosedTextWithBracket =
			(from openBracket in BaseGrammars.OpenSquareBracket
			 from content in Parse.CharExcept(']').Many().Text()
			 from closedBracket in BaseGrammars.ClosedSquareBracket
			 select string.Concat(openBracket, content, closedBracket)).Token();

		private static readonly Parser<string> ParenthesisEnclosedText =
			(from openParenthesis in BaseGrammars.OpenParenthesis
			 from content in Parse.CharExcept(')').Many().Text()
			 from closedParenthesis in BaseGrammars.ClosedParenthesis
			 select content).Token();

		private static readonly Parser<string> ParenthesisEnclosedTextWithParenthesis =
			(from openParenthesis in BaseGrammars.OpenParenthesis
			 from content in Parse.CharExcept(')').Many().Text()
			 from closedParenthesis in BaseGrammars.ClosedParenthesis
			 select string.Concat(openParenthesis, content, closedParenthesis)).Token();

		#endregion

		#region normalized name parser
		private static readonly Parser<string> FileExtensionParser =
			from separator in Parse.Char('.').Once().Text()
			from extension in Parse.LetterOrDigit.AtLeastOnce().Text()
			select string.Concat(separator, extension);

		/// <summary>
		/// A static parser that accepts normalized file names
		/// </summary>
		public static readonly Parser<FansubFile> NormalizedFileNameParser =
			from seriesName in Parse.CharExcept('(').AtLeastOnce().Text().Token()
			from openParenthesis in BaseGrammars.OpenParenthesis
			from episodeNumber in Parse.Number
			from closedParenthesis in BaseGrammars.ClosedParenthesis
			from extension in FileExtensionParser
			select new FansubFile(string.Empty, seriesName.Trim(), int.Parse(episodeNumber, CultureInfo.InvariantCulture), extension);
		#endregion

		#region file name lexers
		private static readonly Parser<IEnumerable<string>> TagLexerWithBrackets =
			SquareBracketEnclosedTextWithBracket.Or(ParenthesisEnclosedTextWithParenthesis).Many();

		private static readonly Parser<IEnumerable<string>> GrindTagsWithBracketsOutOfMajorContent =
			from fansubGroup in SquareBracketEnclosedText.Optional()
			from content in Parse.CharExcept(c => c == '[' || c == '(', "Brackets").Many().Text()
			from tags in TagLexerWithBrackets
			select tags;

		private static readonly Parser<string> VersionNumberFromReversedString =
			from digit in Parse.Digit.AtLeastOnce().Text()
			from vChar in Parse.IgnoreCase('v').Once().Text()
			from rest in BaseGrammars.Line.Optional()
			select string.Concat(vChar, digit);
		#endregion

		#region helper functions
		/// <summary>
		/// Attempts to chop off all the tags at the end of a file name
		/// </summary>
		/// <param name="fileName">The file name</param>
		/// <returns>A string with all of the tags removed</returns>
		private static string RemoveEndTags(string fileName)
		{
			var tagResult = GrindTagsWithBracketsOutOfMajorContent.TryParse(fileName);
			string resultingString = fileName;
			if (tagResult.WasSuccessful)
			{
				foreach (var r in tagResult.Value)
				{
					resultingString = resultingString.Replace(r, string.Empty);
				}
			}
			return resultingString;
		}

		/// <summary>
		/// Attempts to chop off the front Fansub group tag of the file name
		/// </summary>
		/// <param name="fileName">The file name</param>
		/// <returns>A string with the front fansub tag chopped off</returns>
		private static string RemoveFansubTag(string fileName)
		{
			var squareBracketResult = SquareBracketEnclosedTextWithBracket.TryParse(fileName);
			if (squareBracketResult.WasSuccessful)
			{
				return fileName.Replace(squareBracketResult.Value, string.Empty);
			}

			var parenthesisResult = ParenthesisEnclosedTextWithParenthesis.TryParse(fileName);
			if (parenthesisResult.WasSuccessful)
			{
				return fileName.Replace(parenthesisResult.Value, string.Empty);
			}

			return fileName;
		}

		/// <summary>
		/// Attempts to remove the version number after the episode number.
		/// 
		/// This is a bit trickier because there doesn't necessarily need to be a space between the 
		/// episode number and the version number. Furthermore, if the version number is within
		/// an end-tag, we won't be able to detect it.
		/// 
		/// So, in order to accomplish this, we will reverse the string and look for digits followed by 
		/// the letter 'v'
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private static string RemoveVersionNumber(string fileName)
		{
			//Remove the extension first
			var removedExtension = Path.GetFileNameWithoutExtension(fileName);

			//Reverse the string - Let's hope we're not in UTF-16
			var reversedString = new string(removedExtension.Reverse().ToArray());

			var resultOfVersionParse = VersionNumberFromReversedString.TryParse(reversedString);
			if (resultOfVersionParse.WasSuccessful)
			{
				return fileName.Replace(resultOfVersionParse.Value, string.Empty);
			}

			return fileName;
		}

		/// <summary>
		/// Attempts to remove all tags and the extension from the file name. It will also remove the version number
		/// of the file "v*"
		/// </summary>
		/// <param name="fileName">The file name</param>
		/// <returns>A string with the front fansub tag and the leading tags removed</returns>
		private static string RemoveAllTagsAndExtension(string fileName)
		{
			var removedExtension = Path.GetFileNameWithoutExtension(fileName);
			var fansubTagRemoved = RemoveFansubTag(removedExtension);
			var trailingTagsRemoved = RemoveEndTags(fansubTagRemoved);
			return trailingTagsRemoved.Trim();
		}
		#endregion

		#region public factory methods
		/// <summary>
		/// Parse a file name that corresponds to one of the common fansub naming formats. 
		/// 
		/// These naming formats are entirely idiosyncratic, so there's no formal grammar available.
		/// </summary>
		/// <param name="fileName">The file name, without the full path</param>
		/// <returns>A <see cref="FansubFile"/> with all of the parsable information</returns>
		public static FansubFile ParseFansubFile(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return null;
			}

			var normalizedFileResult = NormalizedFileNameParser.TryParse(fileName);
			if (normalizedFileResult.WasSuccessful)
			{
				return normalizedFileResult.Value;
			}

			var fileNameWithReplacedUnderscores = fileName.Replace('_', ' ');

			var fansubGroup = GetFansubGroup(fileNameWithReplacedUnderscores);
			var animeSeries = GetAnimeSeriesName(fileNameWithReplacedUnderscores);
			var epNumber = GetEpisodeNumber(fileNameWithReplacedUnderscores);
			var ext = Path.GetExtension(fileNameWithReplacedUnderscores);

			return new FansubFile(fansubGroup, animeSeries, epNumber, ext);
		}
		#endregion

		#region private helper functions
		/// <summary>
		/// Attempts to get the fansub group from the file name.
		/// </summary>
		/// <param name="fileName">The file name (not full path)</param>
		/// <returns>The fansub group name</returns>
		private static string GetFansubGroup(string fileName)
		{
			var squareBracketResult = SquareBracketEnclosedText.TryParse(fileName);
			if (squareBracketResult.WasSuccessful)
			{
				return squareBracketResult.Value;
			}

			var parenthesisResult = ParenthesisEnclosedText.TryParse(fileName);
			if (parenthesisResult.WasSuccessful)
			{
				return parenthesisResult.Value;
			}

			return string.Empty;
		}

		/// <summary>
		/// Attempts to get the episode number. Returns int.MinValue if it couldn't find it.
		/// </summary>
		/// <remarks>
		/// Works exactly the same as GetAnimeSeries, except that it goes for the complete opposite. It'll first try
		/// to separate out everything by dashes and detect the last element to see if it's a number. Otherwise, it
		/// will separate everything out by spaces and see if the last element is a number. If both of these methods
		/// fail, then we return int.MinValue
		/// </remarks>
		/// <param name="fileName">The file name (not full path)</param>
		/// <returns>The episode number; int.MinValue if it couldn't find it</returns>
		private static int GetEpisodeNumber(string fileName)
		{
			//Chop off the tags
			var withoutTags = RemoveAllTagsAndExtension(fileName);

			//Chop off version number
			var withoutVersionNumber = RemoveVersionNumber(withoutTags);

			int dashDeliminationResult;
			if (TryGetEpisodeNumberUsingDashDelimination(withoutVersionNumber, out dashDeliminationResult))
			{
				return dashDeliminationResult;
			}

			int spaceDeliminatedResult;
			if (TryGetEpisodeNumberUsingSpaceDelimination(withoutVersionNumber, out spaceDeliminatedResult))
			{
				return spaceDeliminatedResult;
			}

			return int.MinValue;
		}

		/// <summary>
		/// Attempts to get the episode number by splitting the string by spaces and assuming the last number is the episode number
		/// </summary>
		/// <param name="fileNameNoTagsUnderscoresVersionNumber">The file name</param>
		/// <param name="episodeNumber">The reference where we write the episode number to</param>
		/// <returns>Whether or not this method succeeded</returns>
		private static bool TryGetEpisodeNumberUsingSpaceDelimination(string fileNameNoTagsUnderscoresVersionNumber, out int episodeNumber)
		{
			var separatedBySpace = fileNameNoTagsUnderscoresVersionNumber.Trim().Split(' ');

			if (!separatedBySpace.Any())
			{
				episodeNumber = int.MinValue;
				return false;
			}

			var lastElement = separatedBySpace.Last();
			return int.TryParse(lastElement, out episodeNumber);
		}

		/// <summary>
		/// Attempts to get the episode number by splitting the string by dashes and assuming the last number is the episode number
		/// </summary>
		/// <param name="fileNameNoTagsUnderscoresVerNumber">The file name</param>
		/// <param name="episodeNumber">The reference where we write the episode number to</param>
		/// <returns>Whether or not this method succeeded</returns>
		private static bool TryGetEpisodeNumberUsingDashDelimination(string fileNameNoTagsUnderscoresVerNumber, out int episodeNumber)
		{
			var dashDeliminatedResult = BaseGrammars.LinesSeparatedByDash.TryParse(fileNameNoTagsUnderscoresVerNumber);
			if (!dashDeliminatedResult.WasSuccessful)
			{
				//Couldn't parse the line at all. Not dash deliminated
				episodeNumber = int.MinValue;
				return false;
			}

			var lastElement = dashDeliminatedResult.Value.Last();
			return int.TryParse(lastElement, out episodeNumber);
		}

		/// <summary>
		/// Attempts to get the anime series name. Returns string.Empty if it couldn't find it.
		/// </summary>
		/// <remarks>
		/// 
		/// Works by:
		/// 1. Chopping off the end tags
		/// 2. Chopping off the Fansub group name
		/// 3. Remove underscores
		/// 4. Check against the most common deliminator: the dash ("-")
		/// 5. If we are able to deliminate the file name by the dash and parse an integer at the end, we'll 
		/// assume that everything before it was a part of the anime series name
		/// 6. Otherwise, if the file name cannot be deliminated by a dash or we couldn't parse an integer 
		/// at the end, then we'll have to split on the spaces after trimming the file name
		/// 7. And then we assume that the last contiguous set of strings is the digit. If this turns out to be false, then 
		/// we simply just re-concatonate everything back together and return that
		/// 
		/// </remarks>
		/// <param name="fileName">The file name (not full path)</param>
		/// <returns>The anime series name</returns>
		private static string GetAnimeSeriesName(string fileName)
		{
			//Chop off the tags
			var withoutTags = RemoveAllTagsAndExtension(fileName);

			//Remove version number
			var withoutVersionNumber = RemoveVersionNumber(withoutTags);

			//Try to use the usual dash deliminated method
			var dashDeliminatedResult = default(string);
			if (TryGetSeriesNameUsingDashDelimination(withoutVersionNumber, out dashDeliminatedResult))
			{
				return dashDeliminatedResult;
			}

			//That didn't work so use the space deliminated method
			var spaceDelminatedResult = default(string);
			if (TryGetSeriesNameUsingSpaceDelimination(withoutVersionNumber, out spaceDelminatedResult))
			{
				return spaceDelminatedResult;
			}

			return withoutTags.Trim();
		}

		/// <summary>
		/// Helper function for <see cref="GetAnimeSeriesName"/>. 
		/// </summary>
		/// <param name="fileNameNoTagsUnderscoresVersionNumber">File name</param>
		/// <param name="seriesName">The reference we should write the result to</param>
		/// <returns>Whether this worked or not</returns>
		private static bool TryGetSeriesNameUsingDashDelimination(string fileNameNoTagsUnderscoresVersionNumber, out string seriesName)
		{
			var dashDeliminatedResult = BaseGrammars.LinesSeparatedByDash.TryParse(fileNameNoTagsUnderscoresVersionNumber);
			if (!dashDeliminatedResult.WasSuccessful)
			{
				//Couldn't parse the line at all. Not dash deliminated
				seriesName = default(string);
				return false;
			}

			//Remove any version numbers first
			var lastElement = dashDeliminatedResult.Value.Last();
			int episodeNumber;
			if (!int.TryParse(lastElement, out episodeNumber))
			{
				//The last element couldn't be parsed into an integer
				seriesName = default(string);
				return false;
			}

			//Go through all but the last element and concatonate everything except the last element
			var stringBuilder = new StringBuilder();
			var resultAsList = dashDeliminatedResult.Value.ToList();
			var lengthOfListWithSeriesName = resultAsList.Count - 1;
			for (var i = 0; i < lengthOfListWithSeriesName; i++)
			{
				stringBuilder.Append(resultAsList[i]);
				if (i + 1 != lengthOfListWithSeriesName)
				{
					stringBuilder.Append("-");
				}
			}

			seriesName = stringBuilder.ToString().Trim();
			return true;
		}

		/// <summary>
		/// Helper function for <see cref="GetAnimeSeriesName"/>. This is different in that we use straight up string manipulation
		/// instead of any parsers
		/// </summary>
		/// <param name="fileNameNoTagsUnderscoresVersionNumber">File name</param>
		/// <param name="seriesName">The reference we should write the result to</param>
		/// <returns>Whether this worked or not</returns>
		private static bool TryGetSeriesNameUsingSpaceDelimination(string fileNameNoTagsUnderscoresVersionNumber, out string seriesName)
		{
			var separatedBySpace = fileNameNoTagsUnderscoresVersionNumber.Trim().Split(' ');

			if (!separatedBySpace.Any())
			{
				//Apparently, we're dealing with the empty string
				seriesName = default(string);
				return false;
			}

			var lastElement = separatedBySpace.Last();
			int episodeNumber;
			if (int.TryParse(lastElement, out episodeNumber))
			{
				//The last element could be parsed. Assume it's an episode number
				var stringBuilder = new StringBuilder();
				foreach (var k in separatedBySpace.Where(t => !t.Equals(lastElement)))
				{
					stringBuilder.AppendFormat("{0} ", k);
				}

				seriesName = stringBuilder.ToString().Trim();
				return true;
			}
			else
			{
				//The last element wasn't a number, so we're going to just reconcat everything
				var stringBuilder = new StringBuilder();
				foreach (var k in separatedBySpace)
				{
					stringBuilder.AppendFormat("{0} ", k);
				}

				seriesName = stringBuilder.ToString().Trim();
				return false;
			}
		}
		#endregion
	}
}
