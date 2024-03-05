using NUnit.Framework;
using FTG.Studios.Robol.VM;
using System;

namespace FTG.Studios.Robol.Test {

    [TestFixture]
    public class SymbolTest {

        [Test]
        public void ConstructorSetsIdentifierAndType() {
            string expected_identifier = "this_here_is_a_variable";
            Type expected_type = typeof(bool);

            Symbol symbol = new Symbol(expected_identifier, expected_type);

            Assert.AreEqual(expected_identifier, symbol.Identifier);
            Assert.AreEqual(expected_type, symbol.Type);
            Assert.AreEqual(null, symbol.Value);
            Assert.AreEqual(false, symbol.IsDefined);
        }

        [Test]
        public void ConstructorSetsAllValues() {
            string expected_identifier = "this_here_is_another_variable";
            Type expected_type = typeof(int);
            int expected_value = 2319;

            Symbol symbol = new Symbol(expected_identifier, expected_type, expected_value);

            Assert.AreEqual(expected_identifier, symbol.Identifier);
            Assert.AreEqual(expected_type, symbol.Type);
            Assert.AreEqual(expected_value, symbol.Value);
            Assert.AreEqual(true, symbol.IsDefined);
        }

        [Test]
        public void CanSetValueOfUndefinedSymbol() {
            string expected_identifier = "an_undefined_variable";
            Type expected_type = typeof(string);

            Symbol symbol = new Symbol(expected_identifier, expected_type);

            string expected_value = "now this here is a string";
            symbol.SetValue(expected_value);

            Assert.AreEqual(expected_identifier, symbol.Identifier);
            Assert.AreEqual(expected_type, symbol.Type);
            Assert.AreEqual(expected_value, symbol.Value);
            Assert.AreEqual(true, symbol.IsDefined);
        }

        [Test]
        public void CanChangeValueOfDefinedSymbol() {
            string expected_identifier = "an_undefined_variable";
            Type expected_type = typeof(string);
            string old_value = "now this here is a string";

            Symbol symbol = new Symbol(expected_identifier, expected_type, old_value);

            string expected_value = "now this here is another string";
            symbol.SetValue(expected_value);

            Assert.AreEqual(expected_identifier, symbol.Identifier);
            Assert.AreEqual(expected_type, symbol.Type);
            Assert.AreEqual(expected_value, symbol.Value);
            Assert.AreEqual(true, symbol.IsDefined);
        }
    }
}
