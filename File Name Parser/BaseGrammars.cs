using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: CLSCompliant(false)]
namespace FileNameParser
{
	internal static class BaseGrammars
	{
		#region basic parsers
		public static readonly Parser<string> Line = Parse.AnyChar.AtLeastOnce().Text();
		public static readonly Parser<string> Identifier = Parse.Letter.AtLeastOnce().Text().Token();
		public static readonly Parser<string> Underscore = Parse.Char('_').AtLeastOnce().Text();
		public static readonly Parser<string> Dash = Parse.Char('-').AtLeastOnce().Text();
		public static readonly Parser<string> OpenParenthesis = Parse.Char('(').Once().Text();
		public static readonly Parser<string> ClosedParenthesis = Parse.Char(')').Once().Text();
		public static readonly Parser<string> OpenSquareBracket = Parse.Char('[').Once().Text();
		public static readonly Parser<string> ClosedSquareBracket = Parse.Char(']').Once().Text();
		#endregion
		#region identifier parsers
		public static readonly Parser<string> IdentifierUntilUnderscore = Parse.Letter.Until(Underscore).Text();
		public static readonly Parser<string> IdentifierUntilUnderscoreOrFullWord = IdentifierUntilUnderscore.Or(Identifier);

		public static readonly Parser<string> IdentifierUntilDash = Parse.Letter.Until(Dash).Text();
		public static readonly Parser<string> IdentifierUntilDashOrFullWord = IdentifierUntilDash.Or(Identifier);
		#endregion
		#region line parsers
		public static readonly Parser<string> LineUntilDash = Parse.AnyChar.Until(Dash).Text();
		public static readonly Parser<string> LineUntilDashOrFullLine = LineUntilDash.Or(Line);

		public static readonly Parser<string> LineUntilUnderscore = Parse.AnyChar.Until(Underscore).Text();
		public static readonly Parser<string> LineUntilUnderscoreOrFullLine = LineUntilUnderscore.Or(Line);

		public static readonly Parser<string> LineUntilOpenSquareBracket = Parse.AnyChar.Until(OpenSquareBracket).Text();
		public static readonly Parser<string> LineUntilOpenParenthesis = Parse.AnyChar.Until(OpenParenthesis).Text();
		public static readonly Parser<string> LineUntilSquareBracketOrParenthesis = LineUntilOpenSquareBracket.Or(LineUntilOpenParenthesis).Text();

		public static readonly Parser<string> LineUntilDigit = Parse.AnyChar.Until(Parse.Number).Text();
		public static readonly Parser<string> LineUntilDigitOrFullLine = LineUntilDigit.Or(Line);
		#endregion
		#region lexers
		#region lexers by identifiers
		public static readonly Parser<IEnumerable<string>> IdentifiersSeparatedByUnderscore = IdentifierUntilUnderscoreOrFullWord.Many();
		public static readonly Parser<IEnumerable<string>> IdentifiersSeparatedByDash = IdentifierUntilDashOrFullWord.Many();
		#endregion
		#region lexers by line
		public static readonly Parser<IEnumerable<string>> LinesSeparatedByUnderscore = LineUntilUnderscoreOrFullLine.Many();
		public static readonly Parser<IEnumerable<string>> LinesSeparatedByDash = LineUntilDashOrFullLine.Many();
		#endregion
		#endregion
	}
}