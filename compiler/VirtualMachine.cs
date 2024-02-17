using System;

namespace FTG.Studios.Robol.Compiler
{

	public enum MessageSeverity { Normal, Warning, Error };

	public class VirtualMachine
	{

		readonly ParseTree ast;
		readonly SymbolTable symbols;

		public VirtualMachine(ParseTree ast)
		{
			this.ast = ast;
			symbols = new SymbolTable();
			Library.AddBuiltinFunctionsToSymbolTable(symbols);
		}

		Action<object, MessageSeverity> ConsoleOutput;
		public void RegisterConsoleOutput(Action<object, MessageSeverity> cb) { ConsoleOutput += cb; }
		public void UnregisterConsoleOutput(Action<object, MessageSeverity> cb) { ConsoleOutput -= cb; }

		public void Run()
		{
			// Define functions
			ParseTree.FunctionList list = ast.Root.List;
			while (list != null)
			{
				if (!symbols.IsDeclared(list.Function.Identifier.Value)) symbols.InsertSymbol(list.Function.Identifier.Value, typeof(void), list.Function);
				list = list.List;
			}
			EvaluateProgram(ast.Root);
		}

		/*** PROGRAM ***/

		void EvaluateProgram(ParseTree.Program program)
		{
			ConsoleOutput?.Invoke(EvaluateFunction(program.Main), MessageSeverity.Normal);
		}

		object EvaluateFunction(ParseTree.Function function)
		{
			if (function is ParseTree.BuiltinFunction) return Library.EvaluateBuiltinFunction(function as ParseTree.BuiltinFunction);
			return EvaluateStatementList(function.Body);
		}

		/*** STATEMENTS ***/

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
			if (!symbols.IsDeclared(statement.Identifier.Value)) symbols.InsertSymbol(statement.Identifier.Value, statement.Type);
			Symbol symbol = symbols.GetSymbol(statement.Identifier.Value);
			symbol.SetValue(EvaluateExpression(statement.Expression));
			return null;
		}

		object EvaluateStatement(ParseTree.Assignment statement)
		{
			Symbol symbol = symbols.GetSymbol(statement.Identifier.Value);
			symbol.SetValue(EvaluateExpression(statement.Expression));
			return null;
		}

		/*** EXPRESSIONS ***/

		object EvaluateExpression(ParseTree.Expression expression)
		{
			if (expression is ParseTree.UnaryExpression) return EvaluateExpression(expression as ParseTree.UnaryExpression);
			if (expression is ParseTree.AdditiveExpression) return EvaluateExpression(expression as ParseTree.AdditiveExpression);
			if (expression is ParseTree.MultiplicativeExpression) return EvaluateExpression(expression as ParseTree.MultiplicativeExpression);
			return null;
		}

		float EvaluateExpression(ParseTree.UnaryExpression expression)
		{
			switch (expression.Operator)
			{
				case Syntax.operator_subtraction: return -EvaluatePrimary(expression.Primary as ParseTree.NumberConstant);
			}
			return 0;
		}

		float EvaluateExpression(ParseTree.AdditiveExpression expression)
		{
			float lhs = EvaluateExpression(expression.LeftExpression);
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
			float lhs = (float)EvaluatePrimary(expression.LeftExpression);

			if (expression.Operator == '\0') return lhs;

			float rhs = (float)EvaluatePrimary(expression.RightExpression);
			return (float)Math.Pow(lhs, rhs);
		}

		/*** PRIMARIES ***/

		object EvaluatePrimary(ParseTree.Primary primary)
		{
			if (primary is ParseTree.Identifier) return EvaluatePrimary(primary as ParseTree.Identifier);
			if (primary is ParseTree.FunctionCall) return EvaluatePrimary(primary as ParseTree.FunctionCall);
			if (primary is ParseTree.NumberConstant) return EvaluatePrimary(primary as ParseTree.NumberConstant);
			if (primary is ParseTree.StringConstant) return EvaluatePrimary(primary as ParseTree.StringConstant);
			if (primary is ParseTree.Expression) return EvaluateExpression(primary as ParseTree.Expression);
			return null;
		}

		object EvaluatePrimary(ParseTree.Identifier primary)
		{
			return symbols.GetSymbol(primary.Value).Value;
		}

		object EvaluatePrimary(ParseTree.FunctionCall primary)
		{
			return EvaluateFunctionCall(primary);
		}

		float EvaluatePrimary(ParseTree.NumberConstant primary)
		{
			return primary.Value;
		}

		string EvaluatePrimary(ParseTree.StringConstant primary)
		{
			return primary.Value;
		}

		bool EvaluatePrimary(ParseTree.BooleanConstant primary)
		{
			return primary.Value;
		}

		object EvaluateFunctionCall(ParseTree.FunctionCall call)
		{
			//symbols.PushScope();

			ParseTree.Function function = symbols.GetSymbol(call.Identifier.Value).Value as ParseTree.Function;

			ParseTree.ParameterList plist = function.Parameters;
			ParseTree.ArgumentList alist = call.Arguments;
			while (plist != null && alist != null)
			{
				ParseTree.Parameter param = plist.Parameter;
				ParseTree.Argument arg = alist.Argument;

				symbols.InsertSymbol(param.Identifier.Value, param.Type, EvaluatePrimary(arg.Primary));

				plist = plist.List;
				alist = alist.List;
			}

			object value = EvaluateFunction(function);

			//symbols.PopScope();
			return value;
		}
	}
}