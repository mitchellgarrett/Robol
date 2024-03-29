﻿using System;
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

			ParseTree.Program program = new ParseTree.Program(main, functions, 0, 0);
			ParseTree ast = new ParseTree(program);

			return ast;
		}

		// FunctionList::= Function FunctionList | null
		static ParseTree.FunctionList ParseFunctionList(Queue<Token> tokens)
		{
			if (tokens.Count == 0) return null;

			int line = tokens.Peek().Line;
			int column = tokens.Peek().Column;

			ParseTree.Function function = ParseFunction(tokens);
			if (function == null) return null;

			ParseTree.FunctionList list = ParseFunctionList(tokens);
			return new ParseTree.FunctionList(function, list, line, column);
		}

		// Function ::= ReturnType Identifier( ParameterList ) { StatementList }
		static ParseTree.Function ParseFunction(Queue<Token> tokens)
		{
			Token token = tokens.Peek();

			int line = token.Line;
			int column = token.Column;

			if (!Match(token, TokenType.Keyword) || !Syntax.IsReturnType((Syntax.Keyword)token.Value)) return null;
			tokens.Dequeue();

			ParseTree.Identifier identifier = ParseIdentifier(tokens);

			MatchFail(tokens.Dequeue(), TokenType.OpenParenthesis);

			ParseTree.ParameterList parameters = ParseParameterList(tokens);

			MatchFail(tokens.Dequeue(), TokenType.CloseParenthesis);
			MatchFail(tokens.Dequeue(), TokenType.OpenBrace);

			ParseTree.StatementList statements = ParseStatementList(tokens);

			ParseTree.Function function = new ParseTree.Function(identifier, Syntax.GetType((Syntax.Keyword)token.Value), parameters, statements, line, column);

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

			return new ParseTree.ParameterList(parameter, list, parameter.Line, parameter.Column);
		}

		// Parameter::= VariableType Identifier
		static ParseTree.Parameter ParseParameter(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();

			int line = token.Line;
			int column = token.Column;

			if (!Match(token, TokenType.Keyword) || !Syntax.IsVariableType((Syntax.Keyword)token.Value)) Fail(token, TokenType.Keyword);

			System.Type type = Syntax.GetType((Syntax.Keyword)token.Value);
			ParseTree.Identifier identifier = ParseIdentifier(tokens);

			return new ParseTree.Parameter(type, identifier, line, column);
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

			return new ParseTree.ArgumentList(argument, list, argument.Line, argument.Column);
		}

		// Argument::= Expression
		static ParseTree.Argument ParseArgument(Queue<Token> tokens)
		{
			ParseTree.Expression expression = ParseExpression(tokens);
			return new ParseTree.Argument(expression, expression.Line, expression.Column);
		}

		// StatementList ::= Statement StatementList | null
		static ParseTree.StatementList ParseStatementList(Queue<Token> tokens)
		{
			ParseTree.Statement statement = ParseStatement(tokens);
			if (statement == null) return null;

			ParseTree.StatementList list = ParseStatementList(tokens);
			return new ParseTree.StatementList(statement, list, statement.Line, statement.Column);
		}

		// Statement ::= DeclarationStatement | AssignmentStatement | ExpressionStatement | SelectionStatement | ReturnStatement | EmptyStatement
		public static ParseTree.Statement ParseStatement(Queue<Token> tokens)
		{
			// DeclarationStatement ::= Declaration;
			// If the first token is a variable type, then the statment is a variable declaration
			if (Match(tokens.Peek(), TokenType.Keyword) && Syntax.IsVariableType((Syntax.Keyword)tokens.Peek().Value)) return ParseDeclarationStatement(tokens);

			// AssignmentStatement ::= Assignment;
			// If this first token is an identifier followed by an '=', then the statement is a variable assignment
			else if (Match(tokens.Peek(), TokenType.Identifier)) return ParseAssignmentStatement(tokens);

			// TODO: add parse functioncall (ExpressionStatement)

			// ReturnStatement ::= return Expression;
			// If the first token is the return keyword, then it is a return statement
			else if (Match(tokens.Peek(), TokenType.Keyword, Syntax.Keyword.Return)) return ParseReturnStatement(tokens);

			// SelectionStatement ::= IfStatement
			else if (Match(tokens.Peek(), TokenType.Keyword, Syntax.Keyword.If)) return ParseIfStatement(tokens);


			// EmptyStatement ::= ;
			if (Match(tokens.Peek(), TokenType.Semicolon)) tokens.Dequeue();

			return null;
		}

		// Declaration ::= Type Identifier | Type Identifier = Expression
		static ParseTree.DeclarationStatement ParseDeclarationStatement(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();

			int line = token.Line;
			int column = token.Column;

			// First token must be a valid variable type
			if (!Match(token, TokenType.Keyword) || !Syntax.IsVariableType((Syntax.Keyword)token.Value)) Fail(token, TokenType.Keyword);

			// Second token must be an identifier
			ParseTree.Identifier identifier = ParseIdentifier(tokens);

			// If there is no '=', then the varaible is declarerd but not defined
			// Otherwise, the variable is initialized with the value of the expression following the '='
			ParseTree.Expression expression = null;
			if (Match(tokens.Peek(), TokenType.Assignment))
			{
				tokens.Dequeue();
				expression = ParseExpression(tokens);
			}

			ParseTree.DeclarationStatement statement = new ParseTree.DeclarationStatement(Syntax.GetType((Syntax.Keyword)token.Value), identifier, expression, line, column);

			MatchFail(tokens.Dequeue(), TokenType.Semicolon);

			return statement;
		}

		// Assignment ::= Identifier = Expression
		static ParseTree.AssignmentStatement ParseAssignmentStatement(Queue<Token> tokens)
		{
			// First token must be a variable identifier
			ParseTree.Identifier identifier = ParseIdentifier(tokens);

			// Second token must be an '='
			MatchFail(tokens.Dequeue(), TokenType.Assignment);

			// The '=' must be followed by an expression
			ParseTree.Expression expression = ParseExpression(tokens);
			ParseTree.AssignmentStatement statement = new ParseTree.AssignmentStatement(identifier, expression, identifier.Line, identifier.Column);

			MatchFail(tokens.Dequeue(), TokenType.Semicolon);

			return statement;
		}

		// Return ::= return Expression
		static ParseTree.ReturnStatement ParseReturnStatement(Queue<Token> tokens)
		{
			MatchFail(tokens.Dequeue(), TokenType.Keyword, Syntax.Keyword.Return);
			ParseTree.Expression expression = ParseExpression(tokens);
			ParseTree.ReturnStatement statement = new ParseTree.ReturnStatement(expression, expression.Line, expression.Column);

			MatchFail(tokens.Dequeue(), TokenType.Semicolon);
			return statement;
		}

		// IfStatement ::= if ( Expression ) { StatementList } | ( Expression ) { StatementList } else { StatementList } | ( Expression ) { StatementList } else IfStatement 
		static ParseTree.IfStatement ParseIfStatement(Queue<Token> tokens)
		{
			int line = tokens.Peek().Line;
			int column = tokens.Peek().Column;

			// If statement must begin with 'if (' 
			MatchFail(tokens.Dequeue(), TokenType.Keyword, Syntax.Keyword.If);
			MatchFail(tokens.Dequeue(), TokenType.OpenParenthesis);

			// Parse if condition
			ParseTree.Expression condition = ParseExpression(tokens);

			// If condition must end in ')' and '{' must start body
			MatchFail(tokens.Dequeue(), TokenType.CloseParenthesis);
			MatchFail(tokens.Dequeue(), TokenType.OpenBrace);

			ParseTree.StatementList true_block = ParseStatementList(tokens);

			// Must have matching '}'
			MatchFail(tokens.Dequeue(), TokenType.CloseBrace);

			// Check for else block
			ParseTree.StatementList false_block = null;
			if (Match(tokens.Peek(), TokenType.Keyword, Syntax.Keyword.Else))
			{
				tokens.Dequeue();

				if (Match(tokens.Peek(), TokenType.Keyword, Syntax.Keyword.If))
				{
					ParseTree.IfStatement next_if_statement = ParseIfStatement(tokens);
					false_block = new ParseTree.StatementList(next_if_statement, null, next_if_statement.Line, next_if_statement.Column);
				}
				else
				{
					MatchFail(tokens.Dequeue(), TokenType.OpenBrace);
					false_block = ParseStatementList(tokens);
					MatchFail(tokens.Dequeue(), TokenType.CloseBrace);
				}
			}

			return new ParseTree.IfStatement(condition, true_block, false_block, line, column);
		}

		// Identifier
		static ParseTree.Identifier ParseIdentifier(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();

			int line = token.Line;
			int column = token.Column;

			MatchFail(token, TokenType.Identifier);

			return new ParseTree.Identifier(token.Value as string, line, column);
		}

		// Expression ::= LogicalExpression
		static ParseTree.Expression ParseExpression(Queue<Token> tokens)
		{
			return ParseLogicalExpression(tokens);
		}

		// LogicalExpression ::= LogicalOrExpression
		static ParseTree.LogicalExpression ParseLogicalExpression(Queue<Token> tokens)
		{
			return ParseLogicalOrExpression(tokens);
		}

		// LogicalOrExpression ::= LogicalAndExpression | LogicalAndExpression or Expression
		static ParseTree.LogicalOrExpression ParseLogicalOrExpression(Queue<Token> tokens)
		{
			ParseTree.LogicalAndExpression lhs = ParseLogicalAndExpression(tokens);

			ParseTree.Expression rhs = null;
			if (Match(tokens.Peek(), TokenType.LogicalOrOperator))
			{
				tokens.Dequeue();
				rhs = ParseExpression(tokens);
			}

			return new ParseTree.LogicalOrExpression(lhs, rhs, lhs.Line, lhs.Column);
		}

		// LogicalAndExpression ::= EqualityExpression | EqualityExpression and Expression
		static ParseTree.LogicalAndExpression ParseLogicalAndExpression(Queue<Token> tokens)
		{
			ParseTree.EqualityExpression lhs = ParseEqualityExpression(tokens);

			ParseTree.Expression rhs = null;
			if (Match(tokens.Peek(), TokenType.LogicalAndOperator))
			{
				tokens.Dequeue();
				rhs = ParseExpression(tokens);
			}

			return new ParseTree.LogicalAndExpression(lhs, rhs, lhs.Line, lhs.Column);
		}

		// EqualityExpression ::= RelationalExpression | RelationalExpression ==|!= Expression
		static ParseTree.EqualityExpression ParseEqualityExpression(Queue<Token> tokens)
		{
			ParseTree.RelationalExpression lhs = ParseRelationalExpression(tokens);

			ParseTree.Expression rhs = null;
			string op = null;
			if (Match(tokens.Peek(), TokenType.EqualityOperator))
			{
				op = (string)tokens.Dequeue().Value;
				rhs = ParseExpression(tokens);
			}

			return new ParseTree.EqualityExpression(op, lhs, rhs, lhs.Line, lhs.Column);
		}

		// RelationalExpression ::= ArithmeticExpression | ArithmeticExpression <|>|<=|>= ArithmeticExpression
		static ParseTree.RelationalExpression ParseRelationalExpression(Queue<Token> tokens)
		{
			ParseTree.ArithmeticExpression lhs = ParseArithmeticExpression(tokens);

			ParseTree.ArithmeticExpression rhs = null;
			string op = null;
			if (Match(tokens.Peek(), TokenType.RelationalOperator))
			{
				op = (string)tokens.Dequeue().Value;
				rhs = ParseArithmeticExpression(tokens);
			}

			return new ParseTree.RelationalExpression(op, lhs, rhs, lhs.Line, lhs.Column);
		}

		// ArithmeticExpression ::= AdditiveExpression
		static ParseTree.ArithmeticExpression ParseArithmeticExpression(Queue<Token> tokens)
		{
			return ParseAdditiveExpression(tokens);
		}

		// AdditiveExpression ::= MultiplicativeExpression +|- Expression
		static ParseTree.AdditiveExpression ParseAdditiveExpression(Queue<Token> tokens)
		{
			ParseTree.MultiplicativeExpression lhs = ParseMultiplicativeExpression(tokens);

			ParseTree.Expression rhs = null;
			char op = '\0';
			if (Match(tokens.Peek(), TokenType.AdditiveOperator))
			{
				op = (char)tokens.Dequeue().Value;
				rhs = ParseExpression(tokens);
			}

			return new ParseTree.AdditiveExpression(op, lhs, rhs, lhs.Line, lhs.Column);
		}

		// MultiplicativeExpression ::= ExponentialExpression *|/|% Expression
		static ParseTree.MultiplicativeExpression ParseMultiplicativeExpression(Queue<Token> tokens)
		{
			ParseTree.ExponentialExpression lhs = ParseExponentialExpression(tokens);

			ParseTree.Expression rhs = null;
			char op = '\0';
			if (Match(tokens.Peek(), TokenType.MultiplicativeOperator))
			{
				op = (char)tokens.Dequeue().Value;
				rhs = ParseExpression(tokens);
			}

			return new ParseTree.MultiplicativeExpression(op, lhs, rhs, lhs.Line, lhs.Column);
		}

		// ExponentialExpression::= Primary ^ Primary
		static ParseTree.ExponentialExpression ParseExponentialExpression(Queue<Token> tokens)
		{
			ParseTree.Primary lhs = ParsePrimary(tokens);

			ParseTree.Primary rhs = null;
			char op = '\0';
			if (Match(tokens.Peek(), TokenType.ExponentialOperator))
			{
				op = (char)tokens.Dequeue().Value;
				rhs = ParsePrimary(tokens);
			}

			return new ParseTree.ExponentialExpression(op, lhs, rhs, lhs.Line, lhs.Column);
		}

		// Primary ::= Identifier | FunctionCall | (Expression) | Constant | UnaryOperator Primary
		static ParseTree.Primary ParsePrimary(Queue<Token> tokens)
		{
			if (tokens.Peek().Type.IsConstant()) return ParseConstant(tokens);

			Token token = tokens.Dequeue();
			if (Match(token, TokenType.Identifier))
			{
				ParseTree.Identifier identifier = new ParseTree.Identifier(token.Value as string, token.Line, token.Column);
				if (Match(tokens.Peek(), TokenType.OpenParenthesis))
				{ // FunctionCall
					MatchFail(tokens.Dequeue(), TokenType.OpenParenthesis);
					ParseTree.FunctionCall call = new ParseTree.FunctionCall(identifier, ParseArgumentList(tokens), token.Line, token.Column);
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

			if (!Match(token, TokenType.UnaryOperator) && !Match(token, TokenType.AdditiveOperator, Syntax.operator_subtraction)) Fail(token, TokenType.UnaryOperator);
			ParseTree.Primary primary = ParsePrimary(tokens);
			return new ParseTree.UnaryExpression((char)token.Value, primary, token.Line, token.Column);
		}

		// Constant ::= IntegerConstant | NumberConstant | StringConstant | BooleanConstant
		static ParseTree.Constant ParseConstant(Queue<Token> tokens)
		{
			Token token = tokens.Dequeue();
			if (!token.Type.IsConstant()) Fail(token, TokenType.Invalid, "Constant");

			if (Match(token, TokenType.IntegerConstant))
			{
				return new ParseTree.IntegerConstant((int)token.Value, token.Line, token.Column);
			}

			if (Match(token, TokenType.NumberConstant))
			{
				return new ParseTree.NumberConstant((float)token.Value, token.Line, token.Column);
			}

			if (Match(token, TokenType.StringConstant))
			{
				return new ParseTree.StringConstant(token.Value as string, token.Line, token.Column);
			}

			if (Match(token, TokenType.BooleanConstant))
			{
				Syntax.Keyword keyword = (Syntax.Keyword)token.Value;
				if (keyword.IsBooleanConstant())
					return new ParseTree.BooleanConstant(keyword.GetBooleanValue(), token.Line, token.Column);
			}

			Fail(token, TokenType.Invalid, "Constant");
			return null;
		}

		static bool Match(Token token, TokenType expectedType)
		{
			return token.Type == expectedType;
		}

		static bool Match(Token token, TokenType expectedType, object expectedValue)
		{
			return token.Type == expectedType && token.Value.Equals(expectedValue);
		}

		static void Fail(Token token)
		{
			System.Console.WriteLine("Invalid token: " + token);
			Environment.Exit(0);
		}

		static void Fail(Token token, TokenType expectedType)
		{
			System.Console.WriteLine($"Invalid token: {token} expected type: {expectedType}");
			Environment.Exit(0);
		}

		static void Fail(Token token, TokenType expectedType, object expectedValue)
		{
			System.Console.WriteLine($"Invalid token: {token} expected type: {expectedType}, expected value: {expectedValue}");
			Environment.Exit(0);
		}

		static void MatchFail(Token token, TokenType expectedType)
		{
			if (!Match(token, expectedType)) Fail(token, expectedType);
		}

		static void MatchFail(Token token, TokenType expectedType, object expectedValue)
		{
			if (!Match(token, expectedType, expectedValue)) Fail(token, expectedType, expectedValue);
		}
	}
}