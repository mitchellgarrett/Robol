using System;
using System.IO;
using FTG.Studios.Robol.Compiler;
using FTG.Studios.Robol.VirtualMachine;
using Internal;

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

		ParseTree output = Compiler.Compile(source);

		Console.WriteLine("\nParse tree:");
		Console.WriteLine(output);

		VirtualMachine vm = new VirtualMachine(output);

		vm.RegisterConsoleOutput((o, m) =>
		{
			Console.WriteLine(o);
		});
		vm.Run();
	}
}