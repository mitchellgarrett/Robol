using System;

namespace FTG.Studios.Robol.Compiler
{

	public static class CodeGenerator
	{
		const string TEMPORARY_VARIABLE_PREFIX = "_t";
		const string TEMPORARY_LABEL_PREFIX = "_L";

		static int nextTemporaryVariableIndex;
		static int nextTemporaryLabelIndex;
		static string indentation;

		/// <summary>
		/// Generates three-address code for the given parse tree.
		/// </summary>
		/// <returns>Three-address code</returns>
		public static string Generate(ParseTree program)
		{
			nextTemporaryVariableIndex = 0;
			nextTemporaryLabelIndex = 0;
			indentation = string.Empty;
			return GenerateProgram(program.Root);
		}

		#region Utility Functions
		static string GetCurrentTemporaryVariable()
		{
			return $"{TEMPORARY_VARIABLE_PREFIX}{nextTemporaryVariableIndex - 1}";
		}

		static string GetNextTemporaryVariable()
		{
			return $"{TEMPORARY_VARIABLE_PREFIX}{nextTemporaryVariableIndex++}";
		}

		static string GetCurrentTemporaryLabel()
		{
			return $"{TEMPORARY_LABEL_PREFIX}{nextTemporaryLabelIndex - 1}";
		}

		static string GetNextTemporaryLabel()
		{
			return $"{TEMPORARY_LABEL_PREFIX}{nextTemporaryLabelIndex++}";
		}

		static void IncreaseIndentation()
		{
			indentation += '\t';
		}

		static void DecreaseIndentation()
		{
			indentation = indentation.Substring(0, indentation.Length - 1);
		}
		#endregion

		#region Program
		static string GenerateProgram(ParseTree.Program program)
		{
			string output = GenerateFunction(program.Main);
			return output;
		}

		static string GenerateFunction(ParseTree.Function function)
		{
			string output = $"{function.Identifier.Value}:\n";
			IncreaseIndentation();
			output += GenerateStatementList(function.Body);
			DecreaseIndentation();
			return output;
		}
		#endregion

		#region Statements
		static string GenerateStatementList(ParseTree.StatementList list)
		{
			if (list == null) return string.Empty;
			string output = GenerateStatement(list.Statement);
			if (list.List != null) output += GenerateStatementList(list.List);
			return output;
		}

		static string GenerateStatement(ParseTree.Statement statement)
		{
			if (statement is ParseTree.ReturnStatement) return GenerateReturnStatement(statement as ParseTree.ReturnStatement);
			if (statement is ParseTree.DeclarationStatement) return GenerateDeclarationStatement(statement as ParseTree.DeclarationStatement);
			if (statement is ParseTree.AssignmentStatement) return GenerateAssignmentStatement(statement as ParseTree.AssignmentStatement);
			if (statement is ParseTree.SelectionStatement) return GenerateSelectionStatement(statement as ParseTree.SelectionStatement);

			Console.Error.WriteLine($"ERROR: GenerateStatement did not produce any code ({statement})");
			return string.Empty;
		}

		static string GenerateReturnStatement(ParseTree.ReturnStatement statement)
		{
			string output = GenerateExpression(statement.Expression);
			string temporary = GetCurrentTemporaryVariable();
			output += $"{indentation}ret {temporary}\n";
			return output;
		}

		static string GenerateDeclarationStatement(ParseTree.DeclarationStatement statement)
		{
			string variable = statement.Identifier.Value;

			string output = GenerateExpression(statement.Expression);
			string temporary = GetCurrentTemporaryVariable();

			// Hack to see if expression is just a constant assignment
			/*if (output.Split("\n").Length == 2)
			{
				nextTemporaryVariableIndex--;
				output = output.Substring(output.IndexOf("= "));
				Console.WriteLine("we got em");
			}*/

			output += $"{indentation}{variable} = {temporary}\n";

			return output;
		}

		static string GenerateAssignmentStatement(ParseTree.AssignmentStatement statement)
		{
			string variable = statement.Identifier.Value;

			string output = GenerateExpression(statement.Expression);
			string temporary = GetCurrentTemporaryVariable();

			output += $"{indentation}{variable} = {temporary}\n";

			return output;
		}

		static string GenerateSelectionStatement(ParseTree.SelectionStatement statement)
		{
			if (statement is ParseTree.IfStatement) return GenerateIfStatement(statement as ParseTree.IfStatement);
			return null;
		}

		static string GenerateIfStatement(ParseTree.IfStatement statement)
		{
			string condition = GenerateExpression(statement.Condition);
			string condition_value = GetCurrentTemporaryVariable();

			string true_label = GetNextTemporaryLabel();

			string output = condition;
			output += $"{indentation}if {condition_value} goto {true_label}\n";

			IncreaseIndentation();
			string false_block = GenerateStatementList(statement.FalseBlock);
			string true_block = GenerateStatementList(statement.TrueBlock);
			string end_label = GetNextTemporaryLabel();

			output += false_block;
			output += $"{indentation}goto {end_label}\n";
			output += $"{true_label}:\n";
			output += true_block;
			output += $"{end_label}:\n";
			DecreaseIndentation();

			return output;
		}
		#endregion

		#region Expressions
		static string GenerateExpression(ParseTree.Expression expression)
		{
			return GenerateLogicalExpression(expression as ParseTree.LogicalExpression);
		}

		#region Logical Expressions
		static string GenerateLogicalExpression(ParseTree.LogicalExpression expression)
		{
			return GenerateLogicalOrExpression(expression as ParseTree.LogicalOrExpression);
		}

		static string GenerateLogicalOrExpression(ParseTree.LogicalOrExpression expression)
		{
			string output = GenerateLogicalAndExpression(expression.LeftExpression);
			if (expression.RightExpression == null) return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string result = GetNextTemporaryLabel();
			output += $"{indentation}{result} = {lhs} || {rhs}\n";

			return output;
		}

		static string GenerateLogicalAndExpression(ParseTree.LogicalAndExpression expression)
		{
			string output = GenerateEqualityExpression(expression.LeftExpression);
			if (expression.RightExpression == null) return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string result = GetNextTemporaryLabel();
			output += $"{indentation}{result} = {lhs} && {rhs}\n";

			return output;
		}

		static string GenerateEqualityExpression(ParseTree.EqualityExpression expression)
		{
			string output = GenerateRelationalExpression(expression.LeftExpression);
			if (expression.RightExpression == null) return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string result = GetNextTemporaryLabel();
			output += $"{indentation}{result} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}

		static string GenerateRelationalExpression(ParseTree.RelationalExpression expression)
		{
			string output = GenerateArithmeticExpression(expression.LeftExpression);
			if (expression.RightExpression == null) return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GenerateArithmeticExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string result = GetNextTemporaryVariable();
			output += $"{indentation}{result} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}
		#endregion

		#region Arithmetic Expressions
		static string GenerateArithmeticExpression(ParseTree.ArithmeticExpression expression)
		{
			return GenerateAdditiveExpression(expression as ParseTree.AdditiveExpression);
		}

		static string GenerateAdditiveExpression(ParseTree.AdditiveExpression expression)
		{
			string output = GenerateMultiplicativeExpression(expression.LeftExpression);
			if (expression.Operator == '\0') return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string temporary = GetNextTemporaryVariable();
			output += $"{indentation}{temporary} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}

		static string GenerateMultiplicativeExpression(ParseTree.MultiplicativeExpression expression)
		{
			string output = GenerateExponentialExpression(expression.LeftExpression);
			if (expression.Operator == '\0') return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string temporary = GetNextTemporaryVariable();
			output += $"{indentation}{temporary} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}

		static string GenerateExponentialExpression(ParseTree.ExponentialExpression expression)
		{
			string output = GeneratePrimary(expression.LeftExpression);
			if (expression.Operator == '\0') return output;

			string lhs = GetCurrentTemporaryVariable();
			output += GeneratePrimary(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string temporary = GetNextTemporaryVariable();
			output += $"{indentation}{temporary} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}
		#endregion
		#endregion

		#region Primaries
		static string GeneratePrimary(ParseTree.Primary primary)
		{
			if (primary is ParseTree.Identifier) return GenerateIdentifier(primary as ParseTree.Identifier);
			if (primary is ParseTree.Constant) return GenerateConstant(primary as ParseTree.Constant);
			if (primary is ParseTree.Expression) return GenerateExpression(primary as ParseTree.Expression);
			if (primary is ParseTree.FunctionCall) return GenerateFunctionCall(primary as ParseTree.FunctionCall);

			Console.Error.WriteLine($"ERROR: GeneratePrimary did not produce any code ({primary})");
			return string.Empty;
		}

		static string GenerateIdentifier(ParseTree.Identifier primary)
		{
			string temporary = GetNextTemporaryVariable();
			string value = primary.Value;
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GenerateFunctionCall(ParseTree.FunctionCall primary)
		{
			return $"{indentation}call {primary.Identifier}\n";
		}

		static string GenerateConstant(ParseTree.Constant constant)
		{
			if (constant is ParseTree.IntegerConstant) return GenerateIntegerConstant(constant as ParseTree.IntegerConstant);
			if (constant is ParseTree.NumberConstant) return GenerateNumberConstant(constant as ParseTree.NumberConstant);
			if (constant is ParseTree.StringConstant) return GenerateStringConstant(constant as ParseTree.StringConstant);
			if (constant is ParseTree.BooleanConstant) return GenerateBooleanConstant(constant as ParseTree.BooleanConstant);

			Console.Error.WriteLine($"ERROR: GenerateConstant did not produce any code ({constant})");
			return null;
		}

		static string GenerateIntegerConstant(ParseTree.IntegerConstant constant)
		{
			string temporary = GetNextTemporaryVariable();
			string value = constant.Value.ToString();
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GenerateNumberConstant(ParseTree.NumberConstant constant)
		{
			string temporary = GetNextTemporaryVariable();
			string value = constant.Value.ToString();
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GenerateStringConstant(ParseTree.StringConstant constant)
		{
			string temporary = GetNextTemporaryVariable();
			string value = $"\"{constant.Value}\"";
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GenerateBooleanConstant(ParseTree.BooleanConstant constant)
		{
			string temporary = GetNextTemporaryVariable();
			string value = constant.Value.ToString().ToLower();
			return $"{indentation}{temporary} = {value}\n";
		}
		#endregion
	}
}