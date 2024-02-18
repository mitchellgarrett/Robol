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
		public interface ASTNode { }

		public class Program : ASTNode
		{

			public Function Main;
			public FunctionList List;

			public Program(Function main, FunctionList list)
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

			public Function Function;
			public FunctionList List;

			public FunctionList(Function statement, FunctionList list)
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

			public Identifier Identifier;
			public Type ReturnType;
			public ParameterList Parameters;
			public StatementList Body;

			public Function(Identifier identifer, Type returnType, ParameterList parameters, StatementList body)
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
			public BuiltinFunction(Identifier identifier, Type returnType, ParameterList parameters) : base(identifier, returnType, parameters, null) { }

			public override string ToString()
			{
				return $"{ReturnType} {Identifier} ({Parameters}): <builtin>";
			}
		}

		public class ParameterList : ASTNode
		{

			public Parameter Parameter;
			public ParameterList List;

			public ParameterList(Parameter parameter, ParameterList list)
			{
				this.Parameter = parameter;
				this.List = list;
			}

			private ParameterList() { }

			public ParameterList(params (Type type, string identifier)[] pairs)
			{
				ParameterList list = this;
				for (int i = 0; i < pairs.Length; ++i)
				{
					list.Parameter = new Parameter(pairs[i].type, new Identifier(pairs[i].identifier));
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

			public Type Type;
			public Identifier Identifier;

			public Parameter(Type type, Identifier identifier)
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

			public Argument Argument;
			public ArgumentList List;

			public ArgumentList(Argument argument, ArgumentList list)
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

			public Primary Primary;

			public Argument(Primary primary)
			{
				this.Primary = primary;
			}

			public override string ToString()
			{
				return $"{Primary}";
			}
		}
		#endregion

		#region Statements
		public interface Statement : ASTNode { }

		public class StatementList : ASTNode
		{

			public Statement Statement;
			public StatementList List;

			public StatementList(Statement statement, StatementList list)
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

			public Type Type;
			public Identifier Identifier;
			public Expression Expression;

			public Declaration(Type type, Identifier identifier, Expression expression)
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

			public Identifier Identifier;
			public Expression Expression;

			public Assignment(Identifier identifier, Expression expression)
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

			public Expression Expression;

			public Return(Expression expression)
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
		public interface Expression : ASTNode, Primary { }

		public class UnaryExpression : Expression
		{

			public char Operator;
			public Primary Primary;

			public UnaryExpression(char op, Primary exp)
			{
				this.Operator = op;
				this.Primary = exp;
			}

			public override string ToString()
			{
				return Operator + Primary.ToString();
			}
		}

		public class AdditiveExpression : Expression
		{

			public char Operator;
			public MultiplicativeExpression LeftExpression;
			public Expression RightExpression;

			public AdditiveExpression(char op, MultiplicativeExpression lhs, Expression rhs)
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

			public char Operator;
			public ExponentialExpression LeftExpression;
			public Expression RightExpression;

			public MultiplicativeExpression(char op, ExponentialExpression lhs, Expression rhs)
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

			public char Operator;
			public Primary LeftExpression;
			public Primary RightExpression;

			public ExponentialExpression(char op, Primary lhs, Primary rhs)
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
		#endregion

		#region Primaries
		public interface Primary : ASTNode { }

		public class Identifier : Primary
		{

			public string Value;

			public Identifier(string value)
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

			public Identifier Identifier;
			public ArgumentList Arguments;

			public FunctionCall(Identifier identifier, ArgumentList arguments)
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

			public int Value;

			public IntegerConstant(int value)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return Value.ToString();
			}
		}

		public class NumberConstant : Primary
		{

			public float Value;

			public NumberConstant(float value)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return Value.ToString();
			}
		}

		public class StringConstant : Primary
		{

			public string Value;

			public StringConstant(string value)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return Value.ToString();
			}
		}

		public class BooleanConstant : Primary
		{

			public bool Value;

			public BooleanConstant(bool value)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				return Value.ToString();
			}
		}
		#endregion
	}
}