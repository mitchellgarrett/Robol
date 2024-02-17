using System.Collections.Generic;

namespace FTG.Studios.Robol.Compiler
{

	public static class Compiler
	{

		public static VirtualMachine Compile(string source)
		{
			List<Token> tokens = Lexer.Tokenize(source);
			ParseTree ast = Parser.Parse(tokens);

			return new VirtualMachine(ast);
		}

	}
}