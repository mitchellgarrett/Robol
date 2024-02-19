using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios.Robol.Compiler
{

	public static class Lexer
	{

		static int line, column, prevColumn;

		public static List<Token> Tokenize(string source)
		{
			List<Token> tokens = new List<Token>();

			Token token;
			line = column = prevColumn = 1;
			string current_word = string.Empty;
			for (int i = 0; i < source.Length; ++i)
			{

				char c = source[i];
				if (char.IsWhiteSpace(c))
				{
					if (c == '\n')
					{
						column = 1;
						prevColumn = column;
						line++;
					}
					else
					{
						column++;
						prevColumn++;
					}

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
					prevColumn = column;
					continue;
				}

				current_word += c;
				column++;
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
				case Syntax.semicolon: return new Token(TokenType.Semicolon, lexeme, line, prevColumn);
				case Syntax.open_brace: return new Token(TokenType.OpenBrace, lexeme, line, prevColumn);
				case Syntax.close_brace: return new Token(TokenType.CloseBrace, lexeme, line, prevColumn);
				case Syntax.open_parenthesis: return new Token(TokenType.OpenParenthesis, lexeme, line, prevColumn);
				case Syntax.close_parenthesis: return new Token(TokenType.CloseParenthesis, lexeme, line, prevColumn);
				case Syntax.comma: return new Token(TokenType.Seperator, lexeme, line, prevColumn);

				// Unary Operators
				case Syntax.operator_negation: return new Token(TokenType.UnaryOperator, lexeme, line, prevColumn);
				case Syntax.operator_complement: return new Token(TokenType.UnaryOperator, lexeme, line, prevColumn);

				// Binary Operators
				case Syntax.operator_assignment: return new Token(TokenType.Assignment, lexeme, line, prevColumn);
				case Syntax.operator_addition: return new Token(TokenType.AdditiveOperator, lexeme, line, prevColumn);
				case Syntax.operator_subtraction: return new Token(TokenType.AdditiveOperator, lexeme, line, prevColumn);

				case Syntax.operator_multiplication: return new Token(TokenType.MultiplicativeOperator, lexeme, line, prevColumn);
				case Syntax.operator_division: return new Token(TokenType.MultiplicativeOperator, lexeme, line, prevColumn);
				case Syntax.operator_modulus: return new Token(TokenType.MultiplicativeOperator, lexeme, line, prevColumn);

				case Syntax.operator_exponent: return new Token(TokenType.ExponentialOperator, lexeme, line, prevColumn);
			}

			return Token.Invalid(line, prevColumn);
		}

		static Token BuildToken(string lexeme)
		{
			Syntax.Keyword keyword;
			if ((keyword = Syntax.GetKeywordType(lexeme)) != Syntax.Keyword.Invalid)
			{
				return new Token(TokenType.Keyword, keyword, line, prevColumn);
			}

			if (Regex.IsMatch(lexeme, Syntax.integer_literal))
			{
				return new Token(TokenType.IntegerConstant, int.Parse(lexeme), line, prevColumn);
			}

			if (Regex.IsMatch(lexeme, Syntax.number_literal))
			{
				return new Token(TokenType.NumberConstant, float.Parse(lexeme), line, prevColumn);
			}

			if (Regex.IsMatch(lexeme, Syntax.string_literal))
			{
				return new Token(TokenType.StringConstant, lexeme, line, prevColumn);
			}

			if (Regex.IsMatch(lexeme, Syntax.identifier))
			{
				return new Token(TokenType.Identifier, lexeme, line, prevColumn);
			}

			return Token.Invalid(line, prevColumn);
		}
	}
}