using System;
using System.Collections.Generic;

namespace FTG.Studios.Robol.VirtualMachine
{

	public class SymbolTable
	{

		public int Scope { get; protected set; }
		Dictionary<string, Symbol> symbols;

		public SymbolTable()
		{
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

	public class Symbol
	{

		public string Identifier { get; protected set; }
		public Type Type { get; protected set; }
		public bool IsDefined { get; protected set; }
		public object Value { get; protected set; }

		public Symbol(string identifier, Type type)
		{
			this.Identifier = identifier;
			this.Type = type;
		}

		public Symbol(string identifier, Type type, object value)
		{
			this.Identifier = identifier;
			this.Type = type;
			this.Value = value;
			IsDefined = true;
		}

		public void SetValue(object value)
		{
			this.Value = value;
			IsDefined = true;
		}

		public override string ToString()
		{
			return $"{Type} : {Identifier}";
		}
	}
}