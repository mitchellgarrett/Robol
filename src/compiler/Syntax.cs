using System;

namespace FTG.Studios.Robol.Compiler
{

	public enum TokenType { Invalid, Semicolon, OpenBrace, CloseBrace, OpenParenthesis, CloseParenthesis, Keyword, Identifier, IntegerConstant, NumberConstant, StringConstant, BooleanConstant, UnaryOperator, ExponentialOperator, MultiplicativeOperator, AdditiveOperator, Assignment, Seperator, LogicalAndOperator, LogicalOrOperator, EqualityOperator, RelationalOperator };

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

		public const string operator_logical_and = "and";
		public const string operator_logical_or = "or";

		public const string operator_equal = "==";
		public const string operator_not_equal = "!=";
		public const string operator_less = "<";
		public const string operator_greater = ">";
		public const string operator_less_equal = "<=";
		public const string operator_greater_equal = ">=";

		public const string identifier = @"^([_a-zA-Z][_a-zA-Z0-9]*)([\._a-zA-Z][_a-zA-Z0-9]*)*$"; // TODO: remove '.' once libraries are supported
		public const string integer_literal = @"^\d+$";
		public const string number_literal = @"^((\d+(\.\d*)?)|(\.\d+))$";
		public const string string_literal = @"^""[a-zA-Z0-9]+""$";

		public static readonly string[] keywords = new string[] { "void", "int", "num", "str", "bool", "true", "false", "return" };

		public enum Keyword { Invalid, Void, Integer, Number, String, Boolean, True, False, Return };

		public static string GetKeyword(Keyword keyword)
		{
			return keywords[(int)keyword - 1];
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

		public static bool IsReturnType(Keyword keyword)
		{
			return IsVariableType(keyword) || keyword == Keyword.Void;
		}

		public static bool IsVariableType(Keyword keyword)
		{
			return keyword == Keyword.Integer || keyword == Keyword.Number || keyword == Keyword.String | keyword == Keyword.Boolean;
		}

		public static Type GetType(Keyword keyword)
		{
			switch (keyword)
			{
				case Keyword.Void: return typeof(void);
				case Keyword.Integer: return typeof(int);
				case Keyword.Number: return typeof(float);
				case Keyword.String: return typeof(string);
				case Keyword.Boolean: return typeof(bool);
			}
			return typeof(object);
		}

		public static bool IsBooleanConstant(this Keyword keyword)
		{
			return keyword == Keyword.True || keyword == Keyword.False;
		}

		public static bool GetBooleanValue(this Keyword keyword)
		{
			return keyword == Keyword.True;
		}

		public static bool IsConstant(this TokenType type)
		{
			return type == TokenType.IntegerConstant || type == TokenType.NumberConstant || type == TokenType.StringConstant || type == TokenType.BooleanConstant;
		}
	}
}