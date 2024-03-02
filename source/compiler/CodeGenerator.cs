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

		static string GenerateStatementList(ParseTree.StatementList list)
		{
			if (list == null) return string.Empty;
			string output = GenerateStatement(list.Statement);
			if (list.List != null) output += GenerateStatementList(list.List);
			return output;
		}

		static string GenerateStatement(ParseTree.Statement statement)
		{
			if (statement == null) return string.Empty;

			if (statement is ParseTree.Return) return GenerateStatement(statement as ParseTree.Return);
			if (statement is ParseTree.Declaration) return GenerateStatement(statement as ParseTree.Declaration);
			//if (statement is ParseTree.Assignment) output = Generate(statement as ParseTree.Assignment);

			Console.Error.WriteLine($"ERROR: GenerateStatement did not produce any code ({statement})");
			return string.Empty;
		}

		static string GenerateStatement(ParseTree.Return statement)
		{
			string output = GenerateExpression(statement.Expression);
			string temporary = GetCurrentTemporaryVariable();
			output += $"{indentation}ret {temporary}\n";
			return output;
		}

		static string GenerateStatement(ParseTree.Declaration statement)
		{
			string variable = statement.Identifier.Value;

			string output = GenerateExpression(statement.Expression);
			string temporary = GetCurrentTemporaryVariable();

			output += $"{indentation}{variable} = {temporary}\n";

			return output;
		}

		static string GenerateExpression(ParseTree.Expression expression)
		{
			if (expression == null) return string.Empty;

			//if (expression is ParseTree.UnaryExpression) return Generate(expression as ParseTree.UnaryExpression);
			if (expression is ParseTree.AdditiveExpression) return GenerateExpression(expression as ParseTree.AdditiveExpression);
			Console.Error.WriteLine($"ERROR: GenerateExpression did not produce any code ({expression})");
			return string.Empty;
		}

		static string GenerateExpression(ParseTree.AdditiveExpression expression)
		{
			string output = GenerateExpression(expression.LeftExpression);
			string lhs = GetCurrentTemporaryVariable();

			if (expression.Operator == '\0') return output;

			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string temporary = GetNextTemporaryVariable();
			output += $"{indentation}{temporary} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}

		static string GenerateExpression(ParseTree.MultiplicativeExpression expression)
		{
			string output = GenerateExpression(expression.LeftExpression);
			string lhs = GetCurrentTemporaryVariable();

			if (expression.Operator == '\0') return output;

			output += GenerateExpression(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string temporary = GetNextTemporaryVariable();
			output += $"{indentation}{temporary} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}

		static string GenerateExpression(ParseTree.ExponentialExpression expression)
		{
			string output = GeneratePrimary(expression.LeftExpression);
			string lhs = GetCurrentTemporaryVariable();

			if (expression.Operator == '\0') return output;

			output += GeneratePrimary(expression.RightExpression);
			string rhs = GetCurrentTemporaryVariable();
			string temporary = GetNextTemporaryVariable();
			output += $"{indentation}{temporary} = {lhs} {expression.Operator} {rhs}\n";

			return output;
		}

		static string GeneratePrimary(ParseTree.Primary primary)
		{
			if (primary == null) return string.Empty;

			if (primary is ParseTree.Identifier) return GeneratePrimary(primary as ParseTree.Identifier);
			if (primary is ParseTree.FunctionCall) return GeneratePrimary(primary as ParseTree.FunctionCall);
			if (primary is ParseTree.IntegerConstant) return GeneratePrimary(primary as ParseTree.IntegerConstant);
			if (primary is ParseTree.NumberConstant) return GeneratePrimary(primary as ParseTree.NumberConstant);
			if (primary is ParseTree.StringConstant) return GeneratePrimary(primary as ParseTree.StringConstant);
			if (primary is ParseTree.BooleanConstant) return GeneratePrimary(primary as ParseTree.BooleanConstant);
			if (primary is ParseTree.Expression) return GeneratePrimary(primary as ParseTree.Expression);

			Console.Error.WriteLine($"ERROR: GeneratePrimary did not produce any code ({primary})");
			return string.Empty;
		}

		static string GeneratePrimary(ParseTree.Identifier primary)
		{
			string temporary = GetNextTemporaryVariable();
			string value = primary.Value;
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GeneratePrimary(ParseTree.FunctionCall primary)
		{
			return $"{indentation}call {primary.Identifier}\n";
		}

		static string GeneratePrimary(ParseTree.IntegerConstant primary)
		{
			string temporary = GetNextTemporaryVariable();
			string value = primary.Value.ToString();
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GeneratePrimary(ParseTree.NumberConstant primary)
		{
			string temporary = GetNextTemporaryVariable();
			string value = primary.Value.ToString();
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GeneratePrimary(ParseTree.StringConstant primary)
		{
			string temporary = GetNextTemporaryVariable();
			string value = $"\"{primary.Value}\"";
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GeneratePrimary(ParseTree.BooleanConstant primary)
		{
			string temporary = GetNextTemporaryVariable();
			string value = primary.Value.ToString();
			return $"{indentation}{temporary} = {value}\n";
		}

		static string GeneratePrimary(ParseTree.Expression primary)
		{
			return GenerateExpression(primary);
		}
	}
}