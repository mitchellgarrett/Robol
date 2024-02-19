using System;
using System.Collections.Generic;
using FTG.Studios.Robol.Compiler;
using Internal;

namespace FTG.Studios.Robol.VirtualMachine
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
						new ParseTree.Identifier(identifier, 0, 0),
						Syntax.GetType(Syntax.Keyword.Number),
						new ParseTree.ParameterList((Syntax.GetType(Syntax.Keyword.Number), "value"))
						)
					);
			}
		}

		public static object EvaluateBuiltinFunction(ParseTree.BuiltinFunction function, SymbolTable symbols)
		{
			if (builtin_functions.TryGetValue(function.Identifier.Value, out Func<object[], object> builtin_function))
			{
				object[] parameters = DeconstructParameterList(function.Parameters, symbols);
				return builtin_function(parameters);
			}
			return null;
		}

		static object[] DeconstructParameterList(ParseTree.ParameterList list, SymbolTable symbols)
		{
			List<object> parameters = new List<object>();
			while (list != null)
			{
				Symbol symbol = symbols.GetSymbol(list.Parameter.Identifier.Value);
				parameters.Add(symbol.Value);
				list = list.List;
			}

			return parameters.ToArray();
		}

		// Built-in functions
		static Dictionary<string, Func<object[], object>> builtin_functions = new Dictionary<string, Func<object[], object>>() {
			{
				"math.sqrt", (parameters) => {
					object value = parameters[0];
					if (value is int) return (float)Math.Sqrt((int)value);
					return (float)Math.Sqrt((float)value);
				}
			}
		};
	}
}