using System;
using System.Collections.Generic;

namespace FTG.Studios.Robol.Compiler
{

	public static class Parser
	{

		public static ParseTree Parse(List<Token> tokens)
		{
			Queue<Token> stream = new Queue<Token>(tokens);

			ParseTree.FunctionList functions = ParseFunctionList(stream);

			// Find main function
			ParseTree.FunctionList list = functions;
			ParseTree.Function main = list.Function;

			while (list.List != null)
			{
				if (main.Identifier.Value.Equals("main")) break;
				list = list.List;
				main = list.Function;
			}

			ParseTree.Program program = new ParseTree.Program(main, functions);
			ParseTree ast = new ParseTree(program);

			return ast;
		}

		// FunctionList::= Function FunctionList | null
		static ParseTree.FunctionList ParseFunctionList(Queue<Token> tokens)
		{
			if (tokens.Count == 0) return null;

			ParseTree.Function function = ParseFunction(tokens);
			if (function == null) return null;

			ParseTree.FunctionList list = ParseFunctionList(tokens);
			return new ParseTree.FunctionList(function, list);
		}

		// Function ::= ReturnType Identifier() { StatementList }
		static ParseTree.Function ParseFunction(Queue<Token> tokens)
		{
			Token token = tokens.Peek();
			if (!Match(token, TokenType.Keyword) || !Syntax.IsReturnType((Syntax.Keyword)token.Value)) return null;
			tokens.Dequeue();

			ParseTree.Identifier identifier = ParseIdentifier(tokens);

			MatchFail(tokens.Dequeue(), TokenType.OpenParenthesis);

			ParseTree.ParameterList parameters = ParseParameterList(tokens);

			MatchFail(tokens.Dequeue(), TokenType.CloseParenthesis);
			MatchFail(tokens.Dequeue(), TokenType.OpenBrace);

			ParseTree.Function function = new ParseTree.Function(identifier, Syntax.GetType((Syntax.Keyword)token.Value), parameters, ParseStatementList(tokens));

			MatchFail(tokens.Dequeue(), TokenType.CloseBrace);

			return function;
		}

		// ParameterList::= Parameter, ParameterList | null
		static ParseTree.ParameterList ParseParameterList(Queue<Token> tokens)
		{
			if (Match(tokens.Peek(), TokenType.CloseParenthesis)) return null;

			ParseTree.Parameter parameter = ParseParameter(tokens);

			ParseTree.ParameterList list = null;
			if (Match(tokens.Peek(), TokenType.Seperator))
			{
				tokens.Dequeue();
				list = ParseParameterList(tokens);
			}

			return new ParseTree.ParameterList(parameter, list);
		}

		// Parameter::= VariableType Identifier
		static ParseTree.Parameter ParseParameter(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();
			if (!Match(token, TokenType.Keyword) || !Syntax.IsVariableType((Syntax.Keyword)token.Value)) Fail(token);

			System.Type type = Syntax.GetType((Syntax.Keyword)token.Value);
			ParseTree.Identifier identifier = ParseIdentifier(tokens);

			return new ParseTree.Parameter(type, identifier);
		}

		// ArgumentList::= Argument, ArgumentList | null
		static ParseTree.ArgumentList ParseArgumentList(Queue<Token> tokens)
		{
			if (Match(tokens.Peek(), TokenType.CloseParenthesis)) return null;

			ParseTree.Argument argument = ParseArgument(tokens);

			ParseTree.ArgumentList list = null;
			if (Match(tokens.Peek(), TokenType.Seperator))
			{
				tokens.Dequeue();
				list = ParseArgumentList(tokens);
			}

			return new ParseTree.ArgumentList(argument, list);
		}

		// Argument::= Primary
		static ParseTree.Argument ParseArgument(Queue<Token> tokens)
		{
			return new ParseTree.Argument(ParsePrimary(tokens));
		}

		// StatementList ::= Statement StatementList | null
		static ParseTree.StatementList ParseStatementList(Queue<Token> tokens)
		{
			ParseTree.Statement statement = ParseStatement(tokens);
			if (statement == null) return null;

			ParseTree.StatementList list = ParseStatementList(tokens);
			return new ParseTree.StatementList(statement, list);
		}

		// Statement ::= Declaration | Assignment | FunctionCall | return Expression
		public static ParseTree.Statement ParseStatement(Queue<Token> tokens)
		{
			if (Match(tokens.Peek(), TokenType.Keyword) && Syntax.IsVariableType((Syntax.Keyword)tokens.Peek().Value)) return ParseDeclaration(tokens);
			if (Match(tokens.Peek(), TokenType.Identifier)) return ParseAssignment(tokens);
			// TODO: add parse functioncall
			if (Match(tokens.Peek(), TokenType.Keyword, Syntax.Keyword.Return)) return ParseReturn(tokens);
			return null;
		}

		// Declaration ::= Type Identifier = Expression;
		static ParseTree.Declaration ParseDeclaration(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();
			if (!Match(token, TokenType.Keyword) || !Syntax.IsVariableType((Syntax.Keyword)token.Value)) Fail(token);

			ParseTree.Identifier identifier = ParseIdentifier(tokens);
			MatchFail(tokens.Dequeue(), TokenType.Assignment);
			ParseTree.Expression expression = ParseExpression(tokens);
			MatchFail(tokens.Dequeue(), TokenType.Semicolon);

			return new ParseTree.Declaration(Syntax.GetType((Syntax.Keyword)token.Value), identifier, expression);
		}

		// Assignment ::= Identifier = Expression;
		static ParseTree.Assignment ParseAssignment(Queue<Token> tokens)
		{
			ParseTree.Identifier identifier = ParseIdentifier(tokens);
			MatchFail(tokens.Dequeue(), TokenType.Assignment);
			ParseTree.Expression expression = ParseExpression(tokens);
			MatchFail(tokens.Dequeue(), TokenType.Semicolon);

			return new ParseTree.Assignment(identifier, expression);
		}

		// Return ::= return Expression;
		static ParseTree.Return ParseReturn(Queue<Token> tokens)
		{
			MatchFail(tokens.Dequeue(), TokenType.Keyword, Syntax.Keyword.Return);
			ParseTree.Expression expression = ParseExpression(tokens);
			ParseTree.Return statement = new ParseTree.Return(expression);
			MatchFail(tokens.Dequeue(), TokenType.Semicolon);

			return statement;
		}

		// Identifier
		static ParseTree.Identifier ParseIdentifier(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();
			MatchFail(token, TokenType.Identifier);

			return new ParseTree.Identifier(token.Value as string);
		}

		// Expression ::= AdditiveExpression
		static ParseTree.Expression ParseExpression(Queue<Token> tokens)
		{
			return ParseAdditiveExpression(tokens);
		}

		// AdditiveExpression ::= MultiplicativeExpression +|- Expression
		static ParseTree.AdditiveExpression ParseAdditiveExpression(Queue<Token> tokens)
		{
			ParseTree.MultiplicativeExpression multiplicative = ParseMultiplicativeExpression(tokens);

			if (!Match(tokens.Peek(), TokenType.AdditiveOperator)) return new ParseTree.AdditiveExpression('\0', multiplicative, null);
			Token token = tokens.Dequeue();

			ParseTree.Expression expression = ParseExpression(tokens);
			return new ParseTree.AdditiveExpression((char)token.Value, multiplicative, expression);
		}

		// MultiplicativeExpression ::= ExponentialExpression *|/|% Expression
		static ParseTree.MultiplicativeExpression ParseMultiplicativeExpression(Queue<Token> tokens)
		{
			ParseTree.ExponentialExpression exponential = ParseExponentialExpression(tokens);

			if (!Match(tokens.Peek(), TokenType.MultiplicativeOperator)) return new ParseTree.MultiplicativeExpression('\0', exponential, null);
			Token token = tokens.Dequeue();

			ParseTree.Expression expression = ParseExpression(tokens);
			return new ParseTree.MultiplicativeExpression((char)token.Value, exponential, expression);
		}

		// ExponentialExpression::= Primary ^ Primary
		static ParseTree.ExponentialExpression ParseExponentialExpression(Queue<Token> tokens)
		{
			ParseTree.Primary lhs = ParsePrimary(tokens);

			if (!Match(tokens.Peek(), TokenType.ExponentialOperator)) return new ParseTree.ExponentialExpression('\0', lhs, null);
			Token token = tokens.Dequeue();

			ParseTree.Primary rhs = ParsePrimary(tokens);
			return new ParseTree.ExponentialExpression((char)token.Value, lhs, rhs);
		}

		// Primary ::= Identifier | FunctionCall | (Expression) | IntegerConstant | NumberConstant | StringConstant | BooleanConstant | UnaryOperator Primary
		static ParseTree.Primary ParsePrimary(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();

			if (Match(token, TokenType.Identifier))
			{
				ParseTree.Identifier identifier = new ParseTree.Identifier(token.Value as string);
				if (Match(tokens.Peek(), TokenType.OpenParenthesis))
				{ // FunctionCall
					MatchFail(tokens.Dequeue(), TokenType.OpenParenthesis);
					ParseTree.FunctionCall call = new ParseTree.FunctionCall(identifier, ParseArgumentList(tokens));
					MatchFail(tokens.Dequeue(), TokenType.CloseParenthesis);
					return call;
				}
				return identifier;
			}

			if (Match(token, TokenType.OpenParenthesis))
			{
				ParseTree.Expression expression = ParseExpression(tokens);
				MatchFail(tokens.Dequeue(), TokenType.CloseParenthesis);
				return expression;
			}

			if (Match(token, TokenType.IntegerConstant))
			{
				return new ParseTree.IntegerConstant((int)token.Value);
			}

			if (Match(token, TokenType.NumberConstant))
			{
				return new ParseTree.NumberConstant((float)token.Value);
			}

			if (Match(token, TokenType.StringConstant))
			{
				return new ParseTree.StringConstant(token.Value as string);
			}

			if (Match(token, TokenType.Keyword)) if (Syntax.IsBooleanConstant((Syntax.Keyword)token.Value)) return new ParseTree.BooleanConstant((Syntax.Keyword)token.Value == Syntax.Keyword.True);
				else Fail(token);

			if (!Match(token, TokenType.UnaryOperator) && !Match(token, TokenType.AdditiveOperator, Syntax.operator_subtraction)) Fail(token);
			ParseTree.Primary primary = ParsePrimary(tokens);
			return new ParseTree.UnaryExpression((char)token.Value, primary);
		}

		static bool Match(Token token, TokenType type)
		{
			return token.Type == type;
		}

		static bool Match(Token token, TokenType type, object value)
		{
			return token.Type == type && token.Value.Equals(value);
		}

		static void Fail(Token token)
		{
			System.Console.WriteLine("Invalid token: " + token);
			Environment.Exit(0);
		}

		static void MatchFail(Token token, TokenType type)
		{
			if (!Match(token, type)) Fail(token);
		}

		static void MatchFail(Token token, TokenType type, object value)
		{
			if (!Match(token, type, value)) Fail(token);
		}
	}
}