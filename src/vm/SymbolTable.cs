using System;
using System.Collections.Generic;
using System.Linq;
using Internal;

namespace FTG.Studios.Robol.VirtualMachine
{

	public class SymbolTable
	{

		readonly SymbolTable parent;
		readonly List<SymbolTable> children;
		readonly Dictionary<string, Symbol> symbols;

		public SymbolTable()
		{
			parent = null;
			children = new List<SymbolTable>();
			symbols = new Dictionary<string, Symbol>();
		}

		SymbolTable(SymbolTable parent)
		{
			this.parent = parent;
			this.parent.children.Add(this);
			children = new List<SymbolTable>();
			symbols = new Dictionary<string, Symbol>();
		}

		public void Clear()
		{
			children.Clear();
			symbols.Clear();
		}

		/// <summary>
		/// Creates a new symbol table as a child of the current table.
		/// </summary>
		/// <returns>The new symbol table.</returns>
		public SymbolTable PushScope()
		{
			return new SymbolTable(this);
		}

		/// <summary>
		/// Removes the current symbol table from its parent.
		/// </summary>
		/// <returns>The parent symbol table.</returns>
		public SymbolTable PopScope()
		{
			parent.children.Remove(this);
			return parent;
		}

		/// <summary>
		/// Creates a new symbol table adjacent to the current table.
		/// </summary>
		/// <returns>The new symbol table.</returns>
		public SymbolTable PushAdjacentScope()
		{
			return new SymbolTable(parent);
		}

		/// <summary>
		/// Removes the current symbol table from its parent and returns its neighbor, or the parent if there are no other child.
		/// </summary>
		/// <returns>The next child of the parent, or the parent.</returns>
		public SymbolTable PopAdjacentScope()
		{
			parent.children.Remove(this);
			if (parent.children.Count > 0) return parent.children.Last();
			return parent;
		}

		public bool InsertSymbol(string identifier, Type type, object value = null)
		{
			if (IsDeclared(identifier))
			{
				return false;
			}
			symbols.Add(identifier, new Symbol(identifier, type, value));
			return true;
		}

		public Symbol GetSymbol(string identifier)
		{
			if (symbols.TryGetValue(identifier, out Symbol symbol)) return symbol;
			if (parent != null) return parent.GetSymbol(identifier);

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
			string output = parent != null ? parent.ToString() + "\n" : string.Empty;
			foreach (Symbol s in symbols.Values) output += $"{s}\n";
			return output;
		}
	}
}