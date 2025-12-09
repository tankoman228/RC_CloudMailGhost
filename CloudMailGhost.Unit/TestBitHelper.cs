using CloudMailGhost.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Unit
{
    [TestClass]
    public class TestBitHelper
    {
        private Random random = new Random();

        [TestMethod]
        public void TestBits()
        {
            byte initial = 0;
            bool[] bools = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                bools[i] = RandomBool;
                BitHelper.SetBitInByte(ref initial, i, bools[i]);
            }

            byte clone = 255;

            for (int i = 0; i < 8; i++)
            {
                var gotBit = BitHelper.GetBitFromByte(initial, i);
                Assert.AreEqual(gotBit, bools[i]);

                BitHelper.SetBitInByte(ref clone, i, bools[i]);
            }

            Assert.AreEqual(initial, clone);
        }

        private bool RandomBool => random.Next() % 2 == 0;
    }
}
