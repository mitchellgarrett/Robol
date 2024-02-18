using System;
using System.Collections.Generic;

namespace FTG.Studios.Robol.VirtualMachine
{

	public class SymbolTable
	{

		readonly SymbolTable parent;
		readonly Dictionary<string, Symbol> symbols;

		public SymbolTable()
		{
			parent = null;
			symbols = new Dictionary<string, Symbol>();
		}

		private SymbolTable(SymbolTable parent)
		{
			this.parent = parent;
			symbols = new Dictionary<string, Symbol>();
		}

		public bool InsertSymbol(string identifier, Type type)
		{
			if (IsDeclared(identifier)) return false;
			symbols.Add(identifier, new Symbol(identifier, type));
			return true;
		}

		public bool InsertSymbol(string identifier, Type type, object value)
		{
			if (IsDeclared(identifier)) return false;
			symbols.Add(identifier, new Symbol(identifier, type, value));
			return true;
		}

		public Symbol GetSymbol(string identifier)
		{
			if (symbols.TryGetValue(identifier, out Symbol symbol)) return symbol;
			//if (parent != null) return parent.GetSymbol(identifier);
			return null;
		}

		public bool IsDeclared(string identifier)
		{
			return symbols.ContainsKey(identifier);
		}

		public bool IsDefined(string identifier)
		{
			if (symbols.TryGetValue(identifier, out Symbol symbol)) return symbol.IsDefined;
			return false;
		}

		public override string ToString()
		{
			string output = "";
			foreach (Symbol s in symbols.Values) output += $"{s}\n";
			return output;
		}
	}
}