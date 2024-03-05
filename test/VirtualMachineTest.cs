using NUnit.Framework;
using FTG.Studios.Robol.VM;
using FTG.Studios.Robol.Compiler;

namespace FTG.Studios.Robol.Test {

    [TestFixture]
    public class VirtualMachineTest {

        [Test]
        public void EmptyProgram_ReturnsNull() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(null, actual);
        }

        [Test]
        public void ReturnStatement_ReturnsIntegerConstant() {
            const int expected = 42;
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return {expected}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnStatement_ReturnsFloatConstant() {
            const float expected = 42.5f;
            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {expected}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnStatement_ReturnsBooleanConstant() {
            const bool expected = true;
            ParseTree program = Compiler.Compiler.Compile($"bool main() {{ return {expected.ToString().ToLower()}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(expected, actual);
        }
    }
}
