using System;

namespace FTG.Studios.Robol.Compiler
{

	public class ParseTree
	{

		public Program Root { get; protected set; }

		public ParseTree(Program root)
		{
			this.Root = root;
		}

		public override string ToString()
		{
			return Root.ToString();
		}

		/*** PROGRAM ***/
		public abstract class ASTNode
		{
			public readonly int Line;
			public readonly int Column;

			protected ASTNode(int line, int column)
			{
				this.Line = line;
				this.Column = column;
			}
		}

		public class Program : ASTNode
		{

			public readonly Function Main;
			public readonly FunctionList List;

			public Program(Function main, FunctionList list, int line, int column) : base(line, column)
			{
				this.Main = main;
				this.List = list;
			}

			public override string ToString()
			{
				return List.ToString();
			}
		}

		#region Functions
		public class FunctionList : ASTNode
		{

			public readonly Function Function;
			public readonly FunctionList List;

			public FunctionList(Function statement, FunctionList list, int line, int column) : base(line, column)
			{
				this.Function = statement;
				this.List = list;
			}

			public override string ToString()
			{
				return Function.ToString() + "\n" + List?.ToString();
			}
		}

		public class Function : ASTNode
		{

			public readonly Identifier Identifier;
			public readonly Type ReturnType;
			public readonly ParameterList Parameters;
			public readonly StatementList Body;

			public Function(Identifier identifer, Type returnType, ParameterList parameters, StatementList body, int line, int column) : base(line, column)
			{
				this.Identifier = identifer;
				this.ReturnType = returnType;
				this.Parameters = parameters;
				this.Body = body;
			}

			public override string ToString()
			{
				return $"{ReturnType} {Identifier} ({Parameters}):\n{Body}";
			}
		}

		public class BuiltinFunction : Function
		{
			public BuiltinFunction(Identifier identifier, Type returnType, ParameterList parameters) : base(identifier, returnType, parameters, null, 0, 0) { }

			public override string ToString()
			{
				return $"{ReturnType} {Identifier} ({Parameters}): <builtin>";
			}
		}

		public class ParameterList : ASTNode
		{

			public Parameter Parameter;
			public ParameterList List;

			public ParameterList(Parameter parameter, ParameterList list, int line, int column) : base(line, column)
			{
				this.Parameter = parameter;
				this.List = list;
			}

			public ParameterList(params (Type type, string identifier)[] pairs) : base(0, 0)
			{
				ParameterList list = this;
				for (int i = 0; i < pairs.Length; ++i)
				{
					list.Parameter = new Parameter(pairs[i].type, new Identifier(pairs[i].identifier, 0, 0), 0, 0);
					if (i < pairs.Length - 1)
					{
						list.List = new ParameterList();
						list = list.List;
					}
				}
			}

			public override string ToString()
			{
				return Parameter.ToString() + ", " + List?.ToString();
			}
		}

		public class Parameter : ASTNode
		{

			public readonly Type Type;
			public readonly Identifier Identifier;

			public Parameter(Type type, Identifier identifier, int line, int column) : base(line, column)
			{
				this.Type = type;
				this.Identifier = identifier;
			}

			public override string ToString()
			{
				return $"{Identifier} ({Type})";
			}
		}

		public class ArgumentList : ASTNode
		{

			public readonly Argument Argument;
			public readonly ArgumentList List;

			public ArgumentList(Argument argument, ArgumentList list, int line, int column) : base(line, column)
			{
				this.Argument = argument;
				this.List = list;
			}

			public override string ToString()
			{
				return Argument.ToString() + ", " + List?.ToString();
			}
		}

		public class Argument : ASTNode
		{

			public readonly Expression Expression;

			public Argument(Expression expression, int line, int column) : base(line, column)
			{
				this.Expression = expression;
			}

			public override string ToString()
			{
				return $"{Expression}";
			}
		}
		#endregion

		#region Statements
		public abstract class Statement : ASTNode
		{
			protected Statement(int line, int column) : base(line, column) { }
		}

		public class StatementList : ASTNode
		{

			public readonly Statement Statement;
			public readonly StatementList List;

			public StatementList(Statement statement, StatementList list, int line, int column) : base(line, column)
			{
				this.Statement = statement;
				this.List = list;
			}

			public override string ToString()
			{
				return Statement.ToString() + "\n" + List?.ToString();
			}
		}

		public class Declaration : Statement
		{

			public readonly Type Type;
			public readonly Identifier Identifier;
			public readonly Expression Expression;

			public Declaration(Type type, Identifier identifier, Expression expression, int line, int column) : base(line, column)
			{
				this.Type = type;
				this.Identifier = identifier;
				this.Expression = expression;
			}

			public override string ToString()
			{
				return $"{Identifier} ({Type}) = {Expression}";
			}
		}

		public class Assignment : Statement
		{

			public readonly Identifier Identifier;
			public readonly Expression Expression;

			public Assignment(Identifier identifier, Expression expression, int line, int column) : base(line, column)
			{
				this.Identifier = identifier;
				this.Expression = expression;
			}

			public override string ToString()
			{
				return Identifier.Value + " = " + Expression.ToString();
			}
		}

		public class Return : Statement
		{

			public readonly Expression Expression;

			public Return(Expression expression, int line, int column) : base(line, column)
			{
				this.Expression = expression;
			}

			public override string ToString()
			{
				return "return <" + Expression.ToString() + ">";
			}
		}
		#endregion

		#region Expressions
		public abstract class Expression : Primary
		{
			protected Expression(int line, int column) : base(line, column) { }
		}

		public class UnaryExpression : Expression
		{

			public readonly char Operator;
			public readonly Primary Primary;

			public UnaryExpression(char op, Primary exp, int line, int column) : base(line, column)
			{
				this.Operator = op;
				this.Primary = exp;
			}

			public override string ToString()
			{
				return Operator + Primary.ToString();
			}
		}

		public class ArithmeticExpression : Expression
		{

			public readonly char Operator;
			public readonly MultiplicativeExpression LeftExpression;
			public readonly Expression RightExpression;

			public ArithmeticExpression(char op, MultiplicativeExpression lhs, Expression rhs, int line, int column) : base(line, column)
			{
				this.Operator = op;
				this.LeftExpression = lhs;
				this.RightExpression = rhs;
			}

			public override string ToString()
			{
				return LeftExpression?.ToString() + (Operator != '\0' ? Operator.ToString() : null) + RightExpression?.ToString();
			}
		}

		public class MultiplicativeExpression : Expression
		{

			public readonly char Operator;
			public readonly ExponentialExpression LeftExpression;
			public readonly Expression RightExpression;

			public MultiplicativeExpression(char op, ExponentialExpression lhs, Expression rhs, int line, int column) : base(line, column)
			{
				this.Operator = op;
				this.LeftExpression = lhs;
				this.RightExpression = rhs;
			}

			public override string ToString()
			{
				return LeftExpression?.ToString() + (Operator != '\0' ? Operator.ToString() : null) + RightExpression?.ToString();
			}
		}

		public class ExponentialExpression : Expression
		{

			public readonly char Operator;
			public readonly Primary LeftExpression;
			public readonly Primary RightExpression;

			public ExponentialExpression(char op, Primary lhs, Primary rhs, int line, int column) : base(line, column)
			{
				this.Operator = op;
				this.LeftExpression = lhs;
				this.RightExpression = rhs;
			}

			public override string ToString()
			{
				return LeftExpression?.ToString() + (Operator != '\0' ? Operator.ToString() : null) + RightExpression?.ToString();
			}
		}

		public class LogicalExpression : Expression
		{
			public readonly LogicalOrExpression Expression;

			public LogicalExpression(LogicalOrExpression expression, int line, int column) : base(line, column)
			{
				this.Expression = expression;
			}

			public override string ToString()
			{
				return Expression.ToString();
			}
		}

		public class LogicalOrExpression : Expression
		{
			public readonly LogicalOrExpression LeftExpression;
			public readonly LogicalAndExpression RightExpression;

			public LogicalOrExpression(LogicalOrExpression lhs, LogicalAndExpression rhs, int line, int column) : base(line, column)
			{
				this.LeftExpression = lhs;
				this.RightExpression = rhs;
			}

			public override string ToString()
			{
				return $"({LeftExpression} or {RightExpression})";
			}
		}

		public class LogicalAndExpression : Expression
		{
			public readonly LogicalAndExpression LeftExpression;
			public readonly LogicalOrExpression RightExpression;

			public LogicalAndExpression(LogicalAndExpression lhs, LogicalOrExpression rhs, int line, int column) : base(line, column)
			{
				this.LeftExpression = lhs;
				this.RightExpression = rhs;
			}

			public override string ToString()
			{
				return $"({LeftExpression} and {RightExpression})";
			}
		}
		#endregion

		#region Primaries
		public abstract class Primary : ASTNode
		{
			protected Primary(int line, int column) : base(line, column) { }
		}

		public class Identifier : Primary
		{

			public readonly string Value;

			public Identifier(string value, int line, int column) : base(line, column)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return Value;
			}
		}

		public class FunctionCall : Primary
		{

			public readonly Identifier Identifier;
			public readonly ArgumentList Arguments;

			public FunctionCall(Identifier identifier, ArgumentList arguments, int line, int column) : base(line, column)
			{
				this.Identifier = identifier;
				this.Arguments = arguments;
			}

			public override string ToString()
			{
				return $"{Identifier}({Arguments})";
			}
		}

		public class IntegerConstant : Primary
		{

			public readonly int Value;

			public IntegerConstant(int value, int line, int column) : base(line, column)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return $"{Value.ToString()} ({Value.GetType()})";
			}
		}

		public class NumberConstant : Primary
		{

			public readonly float Value;

			public NumberConstant(float value, int line, int column) : base(line, column)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return $"{Value.ToString()} ({Value.GetType()})";
			}
		}

		public class StringConstant : Primary
		{

			public readonly string Value;

			public StringConstant(string value, int line, int column) : base(line, column)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return $"{Value.ToString()} ({Value.GetType()})";
			}
		}

		public class BooleanConstant : Primary
		{

			public readonly bool Value;

			public BooleanConstant(bool value, int line, int column) : base(line, column)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return $"{Value.ToString()} ({Value.GetType()})";
			}
		}
		#endregion
	}
}