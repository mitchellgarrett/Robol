using System;
using System.Collections.Generic;
using System.IO;
using FTG.Studios.Robol.Compiler;
using FTG.Studios.Robol.VM;

class Application
{

	public static void Main(string[] args)
	{
		Console.WriteLine("Weclome to the Robol compiler!");

		if (args.Length < 1)
		{
			Console.WriteLine("Usage: robol <file_to_compile>");
			return;
		}

		string file_name = args[0];

		Console.WriteLine($"File: {file_name}\n");

		string source = File.ReadAllText(file_name);

		Console.WriteLine("Source code:");
		Console.WriteLine(source);
		Console.WriteLine();

		List<Token> tokens = Lexer.Tokenize(source);
		Console.WriteLine("Tokens:");
		foreach (Token token in tokens)
		{
			Console.WriteLine(token);
		}
		Console.WriteLine();

		ParseTree output = Compiler.Compile(tokens);

		Console.WriteLine("Parse tree:");
		Console.WriteLine(output);
		Console.WriteLine();

		string tac = CodeGenerator.Generate(output);
		File.WriteAllText(Path.ChangeExtension(file_name, ".tac").Replace("programs", "build"), tac);

		VirtualMachine vm = new VirtualMachine(output);

		vm.RegisterConsoleOutput((o, m) =>
		{
			Console.WriteLine(o);
		});

		object result = vm.Run();
		Console.WriteLine($"Program result: {result}");
	}
}