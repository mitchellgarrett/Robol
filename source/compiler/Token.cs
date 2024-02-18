namespace FTG.Studios.Robol.Compiler
{
	public struct Token
	{
		public readonly int Line;
		public readonly int Column;
		public readonly TokenType Type;
		public readonly object Value;

		public Token(TokenType type, object value, int line, int column)
		{
			this.Type = type;
			this.Value = value;
			this.Line = line;
			this.Column = column;
		}

		public bool IsValid()
		{
			return Type != TokenType.Invalid;
		}

		public static Token Invalid(int line, int column)
		{
			return new Token(TokenType.Invalid, null, line, column);
		}

		public override string ToString()
		{
			return $"<{Line}, {Column}, {Type}, {(Value != null ? Value : "null")}>";
		}
	}
}