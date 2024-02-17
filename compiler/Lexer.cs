using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios.Robol.Compiler
{

	public static class Lexer
	{

		public static List<Token> Tokenize(string source)
		{
			List<Token> tokens = new List<Token>();

			Token token;
			string current_word = string.Empty;
			for (int i = 0; i < source.Length; ++i)
			{

				char c = source[i];
				if (char.IsWhiteSpace(c))
				{
					if (!string.IsNullOrEmpty(current_word))
					{
						tokens.Add(BuildToken(current_word));
						current_word = string.Empty;
					}
					continue;
				}

				token = BuildToken(c);
				if (token.IsValid())
				{
					Token current_token = BuildToken(current_word);
					if (current_token.IsValid())
					{
						tokens.Add(current_token);
					}
					tokens.Add(token);
					current_word = string.Empty;
					continue;
				}

				current_word += c;
			}

			if (!string.IsNullOrEmpty(current_word))
			{
				tokens.Add(BuildToken(current_word));
			}

			return tokens;
		}

		static Token BuildToken(char lexeme)
		{
			switch (lexeme)
			{

				// Puncuation
				case Syntax.semicolon: return new Token(TokenType.Semicolon);
				case Syntax.open_brace: return new Token(TokenType.OpenBrace);
				case Syntax.close_brace: return new Token(TokenType.CloseBrace);
				case Syntax.open_parenthesis: return new Token(TokenType.OpenParenthesis);
				case Syntax.close_parenthesis: return new Token(TokenType.CloseParenthesis);
				case Syntax.comma: return new Token(TokenType.Seperator);

				// Unary Operators
				case Syntax.operator_negation: return new Token(TokenType.UnaryOperator, Syntax.operator_negation);
				case Syntax.operator_complement: return new Token(TokenType.UnaryOperator, Syntax.operator_complement);

				// Binary Operators
				case Syntax.operator_assignment: return new Token(TokenType.Assignment);
				case Syntax.operator_addition: return new Token(TokenType.AdditiveOperator, Syntax.operator_addition);
				case Syntax.operator_subtraction: return new Token(TokenType.AdditiveOperator, Syntax.operator_subtraction);

				case Syntax.operator_multiplication: return new Token(TokenType.MultiplicativeOperator, Syntax.operator_multiplication);
				case Syntax.operator_division: return new Token(TokenType.MultiplicativeOperator, Syntax.operator_division);
				case Syntax.operator_modulus: return new Token(TokenType.MultiplicativeOperator, Syntax.operator_modulus);

				case Syntax.operator_exponent: return new Token(TokenType.ExponentialOperator, Syntax.operator_exponent);
			}

			return Token.Invalid;
		}

		static Token BuildToken(string lexeme)
		{
			Syntax.Keyword keyword;
			if ((keyword = Syntax.GetKeywordType(lexeme)) != Syntax.Keyword.Invalid)
			{
				return new Token(TokenType.Keyword, keyword);
			}

			if (Regex.IsMatch(lexeme, Syntax.number_literal))
			{
				return new Token(TokenType.NumberConstant, float.Parse(lexeme));
			}

			if (Regex.IsMatch(lexeme, Syntax.string_literal))
			{
				return new Token(TokenType.StringConstant, lexeme);
			}

			if (Regex.IsMatch(lexeme, Syntax.identifier))
			{
				return new Token(TokenType.Identifier, lexeme);
			}

			return Token.Invalid;
		}
	}

	public struct Token
	{
		public TokenType Type;
		public object Value;

		public Token(TokenType type)
		{
			this.Type = type;
			Value = null;
		}

		public Token(TokenType type, object value)
		{
			this.Type = type;
			this.Value = value;
		}

		public bool IsValid()
		{
			return Type != TokenType.Invalid;
		}

		public static Token Invalid
		{
			get { return new Token(TokenType.Invalid, null); }
		}

		public override string ToString()
		{
			return $"<{Type.ToString()}, {Value.ToString()}>";
		}
	}
}