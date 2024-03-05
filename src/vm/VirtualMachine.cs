using System;
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
			if (list.Statement is ParseTree.Return) return EvaluateStatement(list.Statement as ParseTree.Return);
			EvaluateStatement(list.Statement);
			return EvaluateStatementList(list.List);
		}

		object EvaluateStatement(ParseTree.Statement statement)
		{
			if (statement is ParseTree.Return) return EvaluateStatement(statement as ParseTree.Return);
			if (statement is ParseTree.Declaration) return EvaluateStatement(statement as ParseTree.Declaration);
			if (statement is ParseTree.Assignment) return EvaluateStatement(statement as ParseTree.Assignment);
			return null;
		}

		object EvaluateStatement(ParseTree.Return statement)
		{
			return EvaluateExpression(statement.Expression);
		}

		object EvaluateStatement(ParseTree.Declaration statement)
		{
			if (!localScope.IsDeclared(statement.Identifier.Value)) localScope.DeclareSymbol(statement.Identifier.Value, statement.Type);
			Symbol symbol = localScope.GetSymbol(statement.Identifier.Value);
			symbol.SetValue(EvaluateExpression(statement.Expression));
			return null;
		}

		object EvaluateStatement(ParseTree.Assignment statement)
		{
			Symbol symbol = localScope.GetSymbol(statement.Identifier.Value);

			if (symbol == null) throw new ArgumentNullException($"({statement.Line}, {statement.Column}): '{statement.Identifier}'", $"Variable not defined");

			symbol.SetValue(EvaluateExpression(statement.Expression));
			return null;
		}
		#endregion

		#region Expressions
		object EvaluateExpression(ParseTree.Expression expression)
		{
			if (expression is ParseTree.UnaryExpression) return EvaluateExpression(expression as ParseTree.UnaryExpression);
			if (expression is ParseTree.ArithmeticExpression) return EvaluateExpression(expression as ParseTree.ArithmeticExpression);
			if (expression is ParseTree.LogicalExpression) return EvaluateExpression(expression as ParseTree.LogicalExpression);
			return null;
		}

		float EvaluateExpression(ParseTree.UnaryExpression expression)
		{
			switch (expression.Operator)
			{
				case Syntax.operator_subtraction:
					return expression.Primary.GetType() == typeof(ParseTree.IntegerConstant)
						? -EvaluateIntegerConstant(expression.Primary as ParseTree.IntegerConstant)
						: -EvaluateNumberConstant(expression.Primary as ParseTree.NumberConstant);
			}
			return 0;
		}

		// TODO: Make these work
		bool EvaluateExpression(ParseTree.LogicalExpression expression)
		{
			return EvaluateExpression(expression.Expression);
		}

		bool EvaluateExpression(ParseTree.LogicalOrExpression expression)
		{
			bool rhs = EvaluateExpression(expression.RightExpression);
			if (expression.LeftExpression != null) rhs = EvaluateExpression(expression.LeftExpression) || rhs;
			return rhs;
		}

		bool EvaluateExpression(ParseTree.LogicalAndExpression expression)
		{
			return EvaluateExpression(expression.LeftExpression) && EvaluateExpression(expression.RightExpression);
		}

		float EvaluateExpression(ParseTree.ArithmeticExpression expression)
		{
			float lhs = (float)EvaluateExpression(expression.LeftExpression);
			if (expression.Operator == '\0') return lhs;
			float rhs = (float)EvaluateExpression(expression.RightExpression);

			switch (expression.Operator)
			{
				case Syntax.operator_addition: return lhs + rhs;
				case Syntax.operator_subtraction: return lhs - rhs;
			}
			return 0;
		}

		float EvaluateExpression(ParseTree.MultiplicativeExpression expression)
		{
			float lhs = (float)EvaluateExpression(expression.LeftExpression);
			if (expression.Operator == '\0') return lhs;
			float rhs = (float)EvaluateExpression(expression.RightExpression);

			switch (expression.Operator)
			{
				case Syntax.operator_multiplication: return lhs * rhs;
				case Syntax.operator_division: return lhs / rhs;
				case Syntax.operator_modulus: return lhs % rhs;
			}
			return 0;
		}

		float EvaluateExpression(ParseTree.ExponentialExpression expression)
		{
			float lhs =
				expression.LeftExpression.GetType() == typeof(ParseTree.IntegerConstant)
				? (float)(int)EvaluatePrimary(expression.LeftExpression)
				: (float)EvaluatePrimary(expression.LeftExpression);

			if (expression.Operator == '\0') return lhs;

			float rhs =
				expression.RightExpression.GetType() == typeof(ParseTree.IntegerConstant)
				? (float)(int)EvaluatePrimary(expression.RightExpression)
				: (float)EvaluatePrimary(expression.RightExpression);

			return (float)Math.Pow(lhs, rhs);
		}
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