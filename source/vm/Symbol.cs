using System;

namespace FTG.Studios.Robol.VirtualMachine
{

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