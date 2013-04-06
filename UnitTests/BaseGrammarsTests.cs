using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileNameParser;
using Sprache;
using System.Collections.Generic;
using System.Text;

namespace UnitTests.Model.Grammars
{
	[TestClass]
	public class BaseGrammarsTests
	{
		private static void ParseWithMapHelper(IDictionary<string, string> inputOutputMap, Parser<string> parser)
		{
			foreach (var t in inputOutputMap)
			{
				var result = parser.TryParse(t.Key);
				Assert.IsTrue(result.WasSuccessful);
				Assert.AreEqual(t.Value, result.Value);
			}
		}
		#region basic parser tests
		#region single character parser tests
		[TestMethod]
		public void UnderscoreParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_", "_"},
				{"_A_", "_"},
				{"__", "__"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.Underscore);
		}

		[TestMethod]
		public void DashParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-", "-"},
				{"-A-", "-"},
				{"--", "--"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.Dash);
		}

		[TestMethod]
		public void OpenParenthesisParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"(", "("},
				{"(A(", "("},
				{"((", "("}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.OpenParenthesis);
		}

		[TestMethod]
		public void ClosedParenthesisParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{")", ")"},
				{")A)", ")"},
				{"))", ")"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.ClosedParenthesis);
		}

		[TestMethod]
		public void OpenSquareBracketParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"[", "["},
				{"[A[", "["},
				{"[[", "["}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.OpenSquareBracket);
		}

		[TestMethod]
		public void ClosedSquareBracketParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"]", "]"},
				{"]A]", "]"},
				{"]]", "]"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.ClosedSquareBracket);
		}
		#endregion

		[TestMethod]
		public void LineParser()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"hello", "hello"},
				{" hello ", " hello "},
				{"hello world", "hello world"},
				{"hello world ", "hello world "},
				{" hello world ", " hello world "}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.Line);
		}

		[TestMethod]
		public void IdentifierParser()
		{
			var inputString = "name";
			var parseResult = BaseGrammars.Identifier.TryParse(inputString);

			Assert.IsTrue(parseResult.WasSuccessful);
			Assert.AreEqual(inputString, parseResult.Value);

			var inputString2 = "name__k";
			var parseResult2 = BaseGrammars.Identifier.TryParse(inputString2);

			Assert.IsTrue(parseResult2.WasSuccessful);
			Assert.AreEqual(inputString, parseResult2.Value);
		}
		#endregion
		#region identifier parser tests
		[TestMethod]
		public void IdentifierUntilUnderscore()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_hello", ""},
				{"hello_world", "hello"},
				{"helloworld_", "helloworld"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.IdentifierUntilUnderscore);
		}

		[TestMethod]
		public void IdentifierUntilUnderscoreOrFullWord()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_hello", ""},
				{"hello_world", "hello"},
				{"helloworld_", "helloworld"},
				{"hello world", "hello"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.IdentifierUntilUnderscoreOrFullWord);
		}

		[TestMethod]
		public void IdentifierUntilDash()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-hello", ""},
				{"hello-world", "hello"},
				{"helloworld-", "helloworld"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.IdentifierUntilDash);
		}

		[TestMethod]
		public void IdentifierUntilDashOrFullWord()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-hello", ""},
				{"hello-world", "hello"},
				{"helloworld-", "helloworld"},
				{"hello world", "hello"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.IdentifierUntilDashOrFullWord);
		}
		#endregion
		#region line parser tests
		[TestMethod]
		public void LineUntilDash()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-hello", ""},
				{"hello-world", "hello"},
				{"helloworld-", "helloworld"},
				{"hello world-", "hello world"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilDash);
		}

		[TestMethod]
		public void LineUntilDashOrFullLine()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-hello", ""},
				{"hello-world", "hello"},
				{"helloworld-", "helloworld"},
				{"hello world", "hello world"},
				{"hello world ", "hello world "}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilDashOrFullLine);
		}

		[TestMethod]
		public void LineUntilUnderscore()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_hello", ""},
				{"hello_world", "hello"},
				{"helloworld_", "helloworld"},
				{"hello world_", "hello world"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilUnderscore);
		}

		[TestMethod]
		public void LineUntilUnderscoreOrFullLine()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_hello", ""},
				{"hello_world", "hello"},
				{"helloworld_", "helloworld"},
				{"hello world", "hello world"},
				{"hello world ", "hello world "}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilUnderscoreOrFullLine);
		}

		[TestMethod]
		public void LineUntilOpenSquareBracket()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"[hello", ""},
				{"hello[world", "hello"},
				{"helloworld[", "helloworld"},
				{"hello world[", "hello world"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilOpenSquareBracket);
		}

		[TestMethod]
		public void LineUntilOpenParenthesis()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"(hello", ""},
				{"hello(world", "hello"},
				{"helloworld(", "helloworld"},
				{"hello world(", "hello world"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilOpenParenthesis);
		}

		[TestMethod]
		public void LineUntilSquareBracketOrParenthesis()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"(hello", ""},
				{"hello(world", "hello"},
				{"helloworld(", "helloworld"},
				{"hello world(", "hello world"},
				{"[hello", ""},
				{"hello[world", "hello"},
				{"helloworld[", "helloworld"},
				{"hello world[", "hello world"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilSquareBracketOrParenthesis);
		}

		[TestMethod]
		public void LineUntilDigit()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"4hello", ""},
				{"hello4world", "hello"},
				{"helloworld4", "helloworld"},
				{"hello world4", "hello world"}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilDigit);
		}

		[TestMethod]
		public void LineUntilDigitOrFullLine()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"4hello", ""},
				{"hello4world", "hello"},
				{"helloworld4", "helloworld"},
				{"hello world4", "hello world"},
				{"hello world ", "hello world "}
			};

			ParseWithMapHelper(inputOutputMap, BaseGrammars.LineUntilDigitOrFullLine);
		}
		#endregion
		#region lexer tests
		#region lexer by identifier tests
		[TestMethod]
		public void IdentifiersSeparateByUnderscore()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_hello world", "helloworld"},
				{" hello _ world", "helloworld"},
				{"hello world", "helloworld"},
				{"hello world_", "helloworld"},
				{"_hello_world", "helloworld"},
				{"hello_world", "helloworld"}
			};

			foreach (var t in inputOutputMap)
			{
				var result = BaseGrammars.IdentifiersSeparatedByUnderscore.TryParse(t.Key);
				Assert.IsTrue(result.WasSuccessful);
				var builder = new StringBuilder();
				foreach (var s in result.Value)
				{
					builder.Append(s);
				}
				Assert.AreEqual(t.Value, builder.ToString());
			}

		}

		[TestMethod]
		public void IdentifiersSeparateByDash()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-hello world", "helloworld"},
				{" hello - world", "helloworld"},
				{"hello world", "helloworld"},
				{"hello world-", "helloworld"},
				{"-hello-world", "helloworld"},
				{"hello-world", "helloworld"}
			};

			foreach (var t in inputOutputMap)
			{
				var result = BaseGrammars.IdentifiersSeparatedByDash.TryParse(t.Key);
				Assert.IsTrue(result.WasSuccessful);
				var builder = new StringBuilder();
				foreach (var s in result.Value)
				{
					builder.Append(s);
				}
				Assert.AreEqual(t.Value, builder.ToString());
			}

		}
		#endregion
		#region lexer by line tests
		[TestMethod]
		public void LinesSeparatedByUnderscore()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"_hello world", "hello world"},
				{" hello _ world", " hello  world"},
				{"hello world", "hello world"},
				{"hello world_", "hello world"},
				{"_hello_world", "helloworld"},
				{"hello_world", "helloworld"}
			};

			foreach (var t in inputOutputMap)
			{
				var result = BaseGrammars.LinesSeparatedByUnderscore.TryParse(t.Key);
				Assert.IsTrue(result.WasSuccessful);
				var builder = new StringBuilder();
				foreach (var s in result.Value)
				{
					builder.Append(s);
				}
				Assert.AreEqual(t.Value, builder.ToString());
			}
		}

		[TestMethod]
		public void LinesSeparatedByDash()
		{
			var inputOutputMap = new Dictionary<string, string>
			{
				{"-hello world", "hello world"},
				{" hello - world", " hello  world"},
				{"hello world", "hello world"},
				{"hello world-", "hello world"},
				{"-hello-world", "helloworld"},
				{"hello-world", "helloworld"}
			};

			foreach (var t in inputOutputMap)
			{
				var result = BaseGrammars.LinesSeparatedByDash.TryParse(t.Key);
				Assert.IsTrue(result.WasSuccessful);
				var builder = new StringBuilder();
				foreach (var s in result.Value)
				{
					builder.Append(s);
				}
				Assert.AreEqual(t.Value, builder.ToString());
			}
		}
		#endregion
		#endregion
	}
}
