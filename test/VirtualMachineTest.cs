using NUnit.Framework;
using FTG.Studios.Robol.VM;
using FTG.Studios.Robol.Compiler;

namespace FTG.Studios.Robol.Test {

    [TestFixture]
    public class VirtualMachineTest {

        #region Return Statements
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
        #endregion

        #region Unary Expressions
        [Test]
        public void UnaryExpression_NegativeInteger() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int x = 100; return -x; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(-100, actual);
        }

        [Test]
        public void UnaryExpression_DoubleNegativeInteger() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = -100; return -value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(int), actual.GetType());
            Assert.AreEqual(100, actual);
        }

        [Test]
        public void UnaryExpression_NegativeNumber() {
            ParseTree program = Compiler.Compiler.Compile($"num main() {{ num value = 100.5; return -value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(-100.5f, actual);
        }

        [Test]
        public void UnaryExpression_DoubleNegativeNumber() {
            ParseTree program = Compiler.Compiler.Compile($"num main() {{ int value = -100.5; return -value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(100.5f, actual);
        }
        #endregion

        #region Integer Arithmetic
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
        #endregion

        #region Number Arithmetic
        [Test]
        public void Arithmetic_NumberAddition_PositiveValues() {
            const float first_value = 57.5f;
            const float second_value = 98.6f;
            const float expected = first_value + second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} + {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberAddition_PositiveAndNegative_PositiveResult() {
            const float first_value = 10.9f;
            const float second_value = -3.9f;
            const float expected = first_value + second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} + {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberAddition_PositiveAndNegative_NegativeResult() {
            const float first_value = 2.5f;
            const float second_value = -3.6f;
            const float expected = first_value + second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} + {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberAddition_NegativeValues() {
            const float first_value = -14.5f;
            const float second_value = -3.5f;
            const float expected = first_value + second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} + {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberSubtraction_PositiveResult() {
            const float first_value = 13.4f;
            const float second_value = 6.7f;
            const float expected = first_value - second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} - {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberSubtraction_NegativeResult() {
            const float first_value = 5.3f;
            const float second_value = 12.6f;
            const float expected = first_value - second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} - {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberSubtraction_DoubleNegative() {
            const float first_value = 32.3f;
            const float second_value = -27.5f;
            const float expected = first_value - second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} - {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberMultiplication() {
            const float first_value = 27.5f;
            const float second_value = 100.5f;
            const float expected = first_value * second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} * {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberDivision_WholeRemainder() {
            const float first_value = 32f;
            const float second_value = 8f;
            const float expected = first_value / second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value}.0 / {second_value}.0; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberDivision_DecimalRemainder() {
            const float first_value = 33f;
            const float second_value = 4f;
            const float expected = first_value / second_value;

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value}.0 / {second_value}.0; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Arithmetic_NumberExponentiation() {
            const float first_value = 57.5f;
            const float second_value = 98.6f;
            float expected = (float)System.Math.Pow(first_value, second_value);

            ParseTree program = Compiler.Compiler.Compile($"num main() {{ return {first_value} ^ {second_value}; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(typeof(float), actual.GetType());
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region If Statements
        [Test]
        public void IfStatement_ExecutesWhenConditionIsTrue() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = 0; if (true) {{ value = 1; }} return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void IfStatement_SkipsWhenConditionIsFalse() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = 0; if (true) {{ value = 1; }} value = 2; return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(2, actual);
        }

        [Test]
        public void IfStatement_ExecutesWhenConditionIsTrue_WithElseBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = 0; if (true) {{ value = 1; }} else {{ value = 2; }} return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void IfStatement_ExecutesElseBranchWhenConditionIsFalse_WithElseBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = 0; if (false) {{ value = 1; }} else {{ value = 2; }} return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(2, actual);
        }

        [Test]
        public void IfStatement_ExecutesWhenConditionIsTrue_WithElseIfBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = 0; if (true) {{ value = 1; }} else if (false) {{ value = 2; }} return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void IfStatement_ExecutesElseIfBranchWhenConditionIsTrue_WithElseIfBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ int value = 0; if (false) {{ value = 1; }} else if (true) {{ value = 2; }} return value; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(2, actual);
        }

        [Test]
        public void IfStatement_ReturnsWhenConditionIsTrue() { 
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (true) {{ return 1; }} return 2; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void IfStatement_SkipsReturnWhenConditionIsFalse() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (false) {{ return 1; }} return 2; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(2, actual);
        }

        [Test]
        public void IfStatement_ReturnsWhenConditionIsTrue_WithElseBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (true) {{ return 1; }} else {{ return 2; }} }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void IfStatement_ReturnsElseBranchWhenConditionIsFalse_WithElseBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (false) {{ return 1; }} else {{ return 2; }} }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(2, actual);
        }

        [Test]
        public void IfStatement_ReturnsWhenConditionIsTrue_WithElseIfBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (true) {{ return 1; }} else if (false) {{ return 2; }} }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void IfStatement_ReturnsElseIfBranchWhenConditionIsFalse_WithElseIfBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (false) {{ return 1; }} else if (true) {{ return 2; }} return 3; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(2, actual);
        }

        [Test]
        public void IfStatement_SkipsBothBranchesWhenBothConditionsAreFalse_WithElseIfBranch() {
            ParseTree program = Compiler.Compiler.Compile($"int main() {{ if (false) {{ return 1; }} else if (false) {{ return 2; }} return 3; }}");
            VirtualMachine vm = new VirtualMachine(program);

            object actual = vm.Run();

            Assert.AreEqual(3, actual);
        }
        #endregion
    }
}
