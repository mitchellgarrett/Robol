using System;

namespace FTG.Studios.Robol.VM
{

	public class Symbol
	{

		public readonly string Identifier;
		public readonly Type Type;
		public bool IsDefined { get; private set; }
		public object Value { get; private set; }

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
			return $"{Type} : {Identifier} ({(Value != null ? Value : "null")})";
		}
	}
}