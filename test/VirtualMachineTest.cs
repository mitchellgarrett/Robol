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

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnStatement_ReturnsFloatConstant() {
            const float expected = 42.5f;
            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {expected}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnStatement_ReturnsBooleanConstant() {
            const bool expected = true;
            ParseTree program = Compiler.Compiler.Compile($"bool main() {{ return {expected.ToString().ToLower()}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(bool), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnStatement_ReturnsVariable() {
            const int expected = 65;
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = {expected}; return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_IntegerAddition_PositiveValues() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 58 + 100; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(158, actual);
        }

        [Test]
        public void Arithmetic_IntegerAddition_PositiveAndNegative_PositiveResult() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return -12 + 20; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(8, actual);
        }

        [Test]
        public void Arithmetic_IntegerAddition_PositiveAndNegative_NegativeResult() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return -12 + 4; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(-8, actual);
        }

        [Test]
        public void Arithmetic_IntegerAddition_NegativeValues() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return -12 + -4; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(-16, actual);
        }

        [Test]
        public void Arithmetic_IntegerSubtraction_PositiveResult() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 32 - 20; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(12, actual);
        }

        [Test]
        public void Arithmetic_IntegerSubtraction_NegativeResult() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 58 - 100; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(-42, actual);
        }

        [Test]
        public void Arithmetic_IntegerSubtraction_DoubleNegative() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 58 - -100; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(158, actual);
        }

        [Test]
        public void Arithmetic_IntegerMultiplication() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 6 * 7; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(42, actual);
        }

        [Test]
        public void Arithmetic_IntegerDivision_WholeRemainder() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 32 / 4; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(8, actual);
        }

        [Test]
        public void Arithmetic_IntegerDivision_DecimalRemainder() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 33 / 4; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(8, actual);
        }

        [Test]
        public void Arithmetic_IntegerExponentiation() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ return 2 ^ 4; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(16, actual);
        }
    }
}
