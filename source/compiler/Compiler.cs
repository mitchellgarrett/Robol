using System.Collections.Generic;

namespace FTG.Studios.Robol.Compiler
{

	public static class Compiler
	{

		public static ParseTree Compile(string source)
		{
			List<Token> tokens = Lexer.Tokenize(source);
			foreach (var t in tokens)
			{
				System.Console.WriteLine(t);
			}
			return Parser.Parse(tokens);
		}

	}
}