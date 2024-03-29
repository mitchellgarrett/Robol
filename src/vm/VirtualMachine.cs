﻿using System;
using System.Collections.Generic;
using FTG.Studios.Robol.Compiler;

namespace FTG.Studios.Robol.VM
{

	public enum MessageSeverity { Normal, Warning, Error };

	public class VirtualMachine
	{

		readonly ParseTree program;
		readonly SymbolTable globalScope;
		SymbolTable localScope;

		public VirtualMachine(ParseTree program)
		{
			this.program = program;
			globalScope = new SymbolTable();
		}

		Action<object, MessageSeverity> ConsoleOutput;
		public void RegisterConsoleOutput(Action<object, MessageSeverity> cb) { ConsoleOutput += cb; }
		public void UnregisterConsoleOutput(Action<object, MessageSeverity> cb) { ConsoleOutput -= cb; }

		public object Run()
		{
			globalScope.Clear();
			Library.AddBuiltinFunctionsToSymbolTable(globalScope);

			// Define functions
			ParseTree.FunctionList list = program.Root.List;
			while (list != null)
			{
				if (!globalScope.IsDeclared(list.Function.Identifier.Value)) globalScope.DefineSymbol(list.Function.Identifier.Value, typeof(void), list.Function);
				list = list.List;
			}

			try
			{
				return EvaluateProgram(program.Root);
			}
			catch (Exception exception)
			{
				ConsoleOutput?.Invoke($"{exception.GetType()}:\n{exception.Message}", MessageSeverity.Error);
			}
			return null;
		}

		#region Program
		object EvaluateProgram(ParseTree.Program program)
		{
			localScope = globalScope.PushScope();
			object result = EvaluateFunction(program.Main);
			localScope = localScope.PopScope();
			return result;
		}

		object EvaluateFunction(ParseTree.Function function)
		{
			if (function is ParseTree.BuiltinFunction) return Library.EvaluateBuiltinFunction(function as ParseTree.BuiltinFunction, localScope);
			return EvaluateStatementList(function.Body);
		}
		#endregion

		#region Statements
		object EvaluateStatementList(ParseTree.StatementList list)
		{
			if (list == null) return null;
			object result = EvaluateStatement(list.Statement);
			if (result != null) return result;
			return EvaluateStatementList(list.List);
		}

		object EvaluateStatement(ParseTree.Statement statement)
		{
			object result = null;

			if (statement is ParseTree.ReturnStatement) result = EvaluateReturnStatement(statement as ParseTree.ReturnStatement);
			else if (statement is ParseTree.DeclarationStatement) EvaluateDeclarationStatement(statement as ParseTree.DeclarationStatement);
			else if (statement is ParseTree.AssignmentStatement) EvaluateAssignmentStatement(statement as ParseTree.AssignmentStatement);
			else if (statement is ParseTree.SelectionStatement) result = EvaluateSelectionStatement(statement as ParseTree.SelectionStatement);
			else throw new ArgumentNullException($"({statement.Line}, {statement.Column}): '{statement}'", $"Invalid statement");

			return result;
		}

		object EvaluateReturnStatement(ParseTree.ReturnStatement statement)
		{
			return EvaluateExpression(statement.Expression);
		}

		void EvaluateDeclarationStatement(ParseTree.DeclarationStatement statement)
		{
			if (!localScope.IsDeclared(statement.Identifier.Value)) localScope.DeclareSymbol(statement.Identifier.Value, statement.Type);
			Symbol symbol = localScope.GetSymbol(statement.Identifier.Value);
			symbol.SetValue(EvaluateExpression(statement.Expression));
		}

		void EvaluateAssignmentStatement(ParseTree.AssignmentStatement statement)
		{
			Symbol symbol = localScope.GetSymbol(statement.Identifier.Value);

			if (symbol == null) throw new ArgumentNullException($"({statement.Line}, {statement.Column}): '{statement.Identifier}'", $"Variable not defined");

			symbol.SetValue(EvaluateExpression(statement.Expression));
		}

		object EvaluateSelectionStatement(ParseTree.SelectionStatement statement)
		{
			if (statement is ParseTree.IfStatement) return EvaluateIfStatement(statement as ParseTree.IfStatement);
			return null;
		}

		object EvaluateIfStatement(ParseTree.IfStatement statement)
		{
			object condition = EvaluateExpression(statement.Condition);
			if (!(condition is bool)) throw new ArgumentNullException($"({statement.Line}, {statement.Column}): '{statement}'", $"Expression does not resolve to boolean");

			if ((bool)condition) return EvaluateStatementList(statement.TrueBlock);
			else return EvaluateStatementList(statement.FalseBlock);
		}
		#endregion

		#region Expressions
		object EvaluateExpression(ParseTree.Expression expression)
		{
			if (expression is ParseTree.UnaryExpression) return EvaluateUnaryExpression(expression as ParseTree.UnaryExpression);
			if (expression is ParseTree.LogicalExpression) return EvaluateLogicalExpression(expression as ParseTree.LogicalExpression);
			return null;
		}

		object EvaluateUnaryExpression(ParseTree.UnaryExpression expression)
		{
			switch (expression.Operator)
			{
				case Syntax.operator_subtraction:
					dynamic value = EvaluatePrimary(expression.Primary);
					return -value;
			}
			return null;
		}

		#region Logical Expressions
		object EvaluateLogicalExpression(ParseTree.LogicalExpression expression)
		{
			return EvaluateLogicalOrExpression(expression as ParseTree.LogicalOrExpression);
		}

		object EvaluateLogicalOrExpression(ParseTree.LogicalOrExpression expression)
		{
			object lhs = EvaluateLogicalAndExpression(expression.LeftExpression);
			if (expression.RightExpression == null) return lhs;

			bool blhs = (bool)lhs;
			bool brhs = (bool)EvaluateExpression(expression.RightExpression);

			return blhs || brhs;
		}

		object EvaluateLogicalAndExpression(ParseTree.LogicalAndExpression expression)
		{
			object lhs = EvaluateEqualityExpression(expression.LeftExpression);
			if (expression.RightExpression == null) return lhs;

			bool blhs = (bool)lhs;
			bool brhs = (bool)EvaluateExpression(expression.RightExpression);

			return blhs && brhs;
		}

		object EvaluateEqualityExpression(ParseTree.EqualityExpression expression)
		{
			object lhs = EvaluateRelationalExpression(expression.LeftExpression);
			if (string.IsNullOrEmpty(expression.Operator)) return lhs;

			object rhs = EvaluateExpression(expression.RightExpression);
			switch (expression.Operator)
			{
				case Syntax.operator_equal: return lhs == rhs;
				case Syntax.operator_not_equal: return lhs != rhs;
			}

			return false;
		}

		object EvaluateRelationalExpression(ParseTree.RelationalExpression expression)
		{
			object lhs = EvaluateArithmeticExpression(expression.LeftExpression);
			if (string.IsNullOrEmpty(expression.Operator)) return lhs;

			dynamic lhs_numeric = lhs;
			dynamic rhs_numeric = EvaluateArithmeticExpression(expression.RightExpression);
			switch (expression.Operator)
			{
				case Syntax.operator_less: return lhs_numeric < rhs_numeric;
				case Syntax.operator_greater: return lhs_numeric > rhs_numeric;
				case Syntax.operator_less_equal: return lhs_numeric <= rhs_numeric;
				case Syntax.operator_greater_equal: return lhs_numeric >= rhs_numeric;
			}

			return false;
		}
		#endregion

		#region Arithmetic Expressions
		object EvaluateArithmeticExpression(ParseTree.ArithmeticExpression expression)
		{
			return EvaluateAdditiveExpression(expression as ParseTree.AdditiveExpression);
		}

		object EvaluateAdditiveExpression(ParseTree.AdditiveExpression expression)
		{
			object lhs = EvaluateMultiplicativeExpression(expression.LeftExpression);
			if (expression.Operator == '\0') return lhs;

			dynamic lhs_numeric = lhs;
			dynamic rhs_numeric = EvaluateExpression(expression.RightExpression);

			switch (expression.Operator)
			{
				case Syntax.operator_addition: return lhs_numeric + rhs_numeric;
				case Syntax.operator_subtraction: return lhs_numeric - rhs_numeric;
			}
			return 0;
		}

		object EvaluateMultiplicativeExpression(ParseTree.MultiplicativeExpression expression)
		{
			object lhs = EvaluateExponentialExpression(expression.LeftExpression);
			if (expression.Operator == '\0') return lhs;

			dynamic lhs_numeric = lhs;
			dynamic rhs_numeric = EvaluateExpression(expression.RightExpression);

			switch (expression.Operator)
			{
				case Syntax.operator_multiplication: return lhs_numeric * rhs_numeric;
				case Syntax.operator_division: return lhs_numeric / rhs_numeric;
				case Syntax.operator_modulus: return lhs_numeric % rhs_numeric;
			}
			return 0;
		}

		object EvaluateExponentialExpression(ParseTree.ExponentialExpression expression)
		{
			object lhs = EvaluatePrimary(expression.LeftExpression);
			if (expression.Operator == '\0') return lhs;

			dynamic lhs_numeric = lhs;
			dynamic rhs_numeric = EvaluatePrimary(expression.RightExpression);

			double result = Math.Pow(lhs_numeric, rhs_numeric);

			if (lhs_numeric is int && rhs_numeric is int) return (int)result;
			return (float)result;
		}
		#endregion
		#endregion

		#region Primaries
		object EvaluatePrimary(ParseTree.Primary primary)
		{
			if (primary is ParseTree.Identifier) return EvaluateIdentifier(primary as ParseTree.Identifier);
			if (primary is ParseTree.Constant) return EvaluateConstant(primary as ParseTree.Constant);
			if (primary is ParseTree.Expression) return EvaluateExpression(primary as ParseTree.Expression);
			if (primary is ParseTree.FunctionCall) return EvaluateFunctionCall(primary as ParseTree.FunctionCall);
			return null;
		}

		object EvaluateIdentifier(ParseTree.Identifier identifier)
		{
			Symbol symbol = localScope.GetSymbol(identifier.Value);
			if (symbol == null) throw new ArgumentNullException($"Variable '{identifier.Value}' does not exist!");
			return symbol.Value;
		}

		object EvaluateConstant(ParseTree.Constant constant)
		{
			if (constant is ParseTree.IntegerConstant) return EvaluateIntegerConstant(constant as ParseTree.IntegerConstant);
			if (constant is ParseTree.NumberConstant) return EvaluateNumberConstant(constant as ParseTree.NumberConstant);
			if (constant is ParseTree.StringConstant) return EvaluateStringConstant(constant as ParseTree.StringConstant);
			if (constant is ParseTree.BooleanConstant) return EvaluateBooleanConstant(constant as ParseTree.BooleanConstant);
			return null;
		}

		int EvaluateIntegerConstant(ParseTree.IntegerConstant constant)
		{
			return constant.Value;
		}

		float EvaluateNumberConstant(ParseTree.NumberConstant constant)
		{
			return constant.Value;
		}

		string EvaluateStringConstant(ParseTree.StringConstant constant)
		{
			return constant.Value;
		}

		bool EvaluateBooleanConstant(ParseTree.BooleanConstant constant)
		{
			return constant.Value;
		}

		object EvaluateFunctionCall(ParseTree.FunctionCall call)
		{
			ParseTree.Function function = localScope.GetSymbol(call.Identifier.Value).Value as ParseTree.Function;

			ParseTree.ParameterList plist = function.Parameters;
			ParseTree.ArgumentList alist = call.Arguments;
			List<(string, Type, object)> parameters = new List<(string, Type, object)>();
			while (plist != null && alist != null)
			{
				ParseTree.Parameter param = plist.Parameter;
				ParseTree.Argument arg = alist.Argument;

				parameters.Add((param.Identifier.Value, param.Type, EvaluateExpression(arg.Expression)));

				plist = plist.List;
				alist = alist.List;
			}

			localScope = localScope.PushAdjacentScope();
			foreach ((string identifier, Type type, object value) in parameters)
			{
				localScope.DefineSymbol(identifier, type, value);
			}

			object result = EvaluateFunction(function);
			localScope = localScope.PopAdjacentScope();

			return result;
		}
		#endregion
	}
}