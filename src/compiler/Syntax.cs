using System;

namespace FTG.Studios.Robol.Compiler
{

	public enum TokenType { Invalid, Semicolon, OpenBrace, CloseBrace, OpenParenthesis, CloseParenthesis, Keyword, Identifier, IntegerConstant, NumberConstant, StringConstant, UnaryOperator, ExponentialOperator, MultiplicativeOperator, AdditiveOperator, Assignment, Seperator };

	public static class Syntax
	{

		public const char semicolon = ';';
		public const char open_brace = '{';
		public const char close_brace = '}';
		public const char open_parenthesis = '(';
		public const char close_parenthesis = ')';
		public const char single_quote = '\'';
		public const char double_quote = '\"';
		public const char period = '.';
		public const char comma = ',';

		public const char operator_assignment = '=';

		public const char operator_addition = '+';
		public const char operator_subtraction = '-';
		public const char operator_multiplication = '*';
		public const char operator_division = '/';
		public const char operator_modulus = '%';
		public const char operator_exponent = '^';

		public const char operator_negation = '!';
		public const char operator_complement = '~';

		public const string conditional_and = "&&";
		public const string conditional_or = "||";

		public const string comparator_equal = "==";
		public const string comparator_not_equal = "!=";
		public const string comparator_less = "<";
		public const string comparator_greater = ">";
		public const string comparator_less_equal = "<=";
		public const string comparator_greater_equal = ">=";

		public const string identifier = @"^([_a-zA-Z][_a-zA-Z0-9]*)([\._a-zA-Z][_a-zA-Z0-9]*)*$"; // TODO: remove '.' once libraries are supported
		public const string integer_literal = @"^\d+$";
		public const string number_literal = @"^((\d+(\.\d*)?)|(\.\d+))$";
		public const string string_literal = @"^""[a-zA-Z0-9]+""$";

		public static readonly string[] keywords = new string[] { "void", "int", "num", "str", "bool", "true", "false", "return" };

		public enum Keyword { Invalid, Void, Integer, Number, String, Boolean, True, False, Return };

		public static string GetKeyword(Keyword k)
		{
			return keywords[(int)k - 1];
		}

		public static Keyword GetKeywordType(string s)
		{
			for (int i = 0; i < keywords.Length; i++)
			{
				if (s == keywords[i])
				{
					return (Keyword)(i + 1);
				}
			}
			return Keyword.Invalid;
		}

		public static bool IsType(string s)
		{
			return IsVariableType(GetKeywordType(s));
		}

		public static bool IsReturnType(Keyword k)
		{
			return IsVariableType(k) || k == Keyword.Void;
		}

		public static bool IsVariableType(Keyword k)
		{
			return k == Keyword.Integer || k == Keyword.Number || k == Keyword.String | k == Keyword.Boolean;
		}

		public static Type GetType(Keyword k)
		{
			switch (k)
			{
				case Keyword.Void: return typeof(void);
				case Keyword.Integer: return typeof(int);
				case Keyword.Number: return typeof(float);
				case Keyword.String: return typeof(string);
				case Keyword.Boolean: return typeof(bool);
			}
			return typeof(object);
		}

		public static bool IsBooleanConstant(Keyword k)
		{
			return k == Keyword.True || k == Keyword.False;
		}
	}
}