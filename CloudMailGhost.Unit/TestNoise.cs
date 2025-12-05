using CloudMailGhost.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Unit
{
    [TestClass]
    public class TestNoise
    {
        [TestMethod]
        public void Test18()
        {
            var noise = NoiseGenerator.GenerateNoise("123", 800, 1, 8, out int niggger);
            foreach (var n in noise)
            {
                if (n > 8 || n < 1) Assert.Fail();
            }
        }
    }
}
