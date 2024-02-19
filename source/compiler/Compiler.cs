using System.Collections.Generic;

namespace FTG.Studios.Robol.Compiler
{

	public static class Compiler
	{

		public static ParseTree Compile(string source)
		{
			List<Token> tokens = Lexer.Tokenize(source);
			return Parser.Parse(tokens);
		}

		public static ParseTree Compile(List<Token> tokens)
		{
			return Parser.Parse(tokens);
		}
	}
}