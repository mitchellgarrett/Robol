using NUnit.Framework;
using FTG.Studios.Robol.VM;
using System;

namespace FTG.Studios.Robol.Test {

    [TestFixture]
    public class SymbolTableTest {

        [Test]
        public void CanDeclareSymbol() {
            SymbolTable table = new SymbolTable();

            string expected_identifier = "a_new_variable";
            Type expected_type = typeof(float);

            Assert.AreEqual(true, table.DeclareSymbol(expected_identifier, expected_type));

            Symbol symbol = table.GetSymbol(expected_identifier);

            Assert.AreEqual(expected_identifier, symbol.Identifier);
            Assert.AreEqual(expected_type, symbol.Type);
            Assert.AreEqual(null, symbol.Value);

            Assert.AreEqual(true, table.IsDeclared(expected_identifier));
            Assert.AreEqual(false, symbol.IsDefined);
            Assert.AreEqual(false, table.IsDefined(expected_identifier));
        }

        [Test]
        public void CanDefineSymbol() {
            SymbolTable table = new SymbolTable();

            string expected_identifier = "a_new_variable";
            Type expected_type = typeof(float);
            float expected_value = 4985.5f;

            Assert.AreEqual(true, table.DefineSymbol(expected_identifier, expected_type, expected_value));

            Symbol symbol = table.GetSymbol(expected_identifier);

            Assert.AreEqual(expected_identifier, symbol.Identifier);
            Assert.AreEqual(expected_type, symbol.Type);
            Assert.AreEqual(expected_value, symbol.Value);

            Assert.AreEqual(true, table.IsDeclared(expected_identifier));
            Assert.AreEqual(true, symbol.IsDefined);
            Assert.AreEqual(true, table.IsDefined(expected_identifier));
        }

        [Test]
        public void CanClearTable() {
            SymbolTable table = new SymbolTable();

            string expected_identifier = "a_new_variable";
            Type expected_type = typeof(void);

            table.DeclareSymbol(expected_identifier, expected_type);

            table.Clear();

            Assert.AreEqual(false, table.IsDeclared(expected_identifier));
            Assert.AreEqual(null, table.GetSymbol(expected_identifier));
        }
    }
}
