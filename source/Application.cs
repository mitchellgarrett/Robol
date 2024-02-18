using System;
using System.IO;
using FTG.Studios.Robol.Compiler;
using FTG.Studios.Robol.VirtualMachine;

class Application
{

	public static void Main(string[] args)
	{
		Console.WriteLine("Weclome to the Robol compiler!");

		string file_name = "programs/example.rbl";

		Console.WriteLine($"File: {file_name}");

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