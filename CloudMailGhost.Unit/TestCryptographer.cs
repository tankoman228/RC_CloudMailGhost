using CloudMailGhost.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Unit
{
    [TestClass]
    public class TestCryptographer
    {
        [TestMethod]
        public void Test16()
        {
            var data = RandomNumberGenerator.GetBytes(RandomNumberGenerator.GetInt32(2048, 2048 * 64));
            var IV = RandomNumberGenerator.GetBytes(16);
            var key = "bibki123123123123123123123123dsfwerf32rfgerывцйвф"; 

            byte[] encoded = Сryptographer.Encode(data, key, IV);
            byte[] decoded = Сryptographer.Decode(encoded, key, IV);

            CollectionAssert.AreEqual(data, decoded);
        }

        [TestMethod]
        public void Test16ShortString()
        {
            var data = RandomNumberGenerator.GetBytes(RandomNumberGenerator.GetInt32(2048, 2048 * 64));
            var IV = RandomNumberGenerator.GetBytes(16);
            var key = "12";

            byte[] encoded = Сryptographer.Encode(data, key, IV);
            byte[] decoded = Сryptographer.Decode(encoded, key, IV);

            CollectionAssert.AreEqual(data, decoded);
        }
    }
}
