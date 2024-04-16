using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using AutoCAD_2022_Plugin1.LogicServices;
using AutoCAD_2022_Plugin1;

namespace TestAutocad
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
            var expected = new System.Exception("No active document!");
            
            Field field = new Field("d1", "Нет", "A4");

            try
            {
                field.Draw();
            }
            catch (System.Exception ex)
            {
                StringAssert.Contains(ex.Message,ToString(), expected.ToString());
            }



        }
    }
}
