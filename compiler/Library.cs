using System;
using System.Collections.Generic;

namespace FTG.Studios.Robol.Compiler
{

	public static class Library
	{

		public static bool IsBuiltinFunction(string identifier)
		{
			return builtin_functions.ContainsKey(identifier);
		}

		public static void AddBuiltinFunctionsToSymbolTable(SymbolTable symbols)
		{
			foreach (string identifier in builtin_functions.Keys)
			{
				symbols.InsertSymbol(
					identifier,
					typeof(void),
					new ParseTree.BuiltinFunction(
						new ParseTree.Identifier(identifier),
						Syntax.GetType(Syntax.Keyword.Number),
						new ParseTree.ParameterList((Syntax.GetType(Syntax.Keyword.Number), "num"))
						)
					);
			}
		}

		public static object EvaluateBuiltinFunction(ParseTree.BuiltinFunction function)
		{
			if (builtin_functions.TryGetValue(function.Identifier.Value, out Func<float, float> f)) return f(4);
			return null;
		}

		// Built-in functions
		static Dictionary<string, Func<float, float>> builtin_functions = new Dictionary<string, Func<float, float>>() {
			{ "math.sqrt", (n) => { return (float)Math.Sqrt(n); } }
		};
	}
}