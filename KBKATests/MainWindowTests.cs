using Microsoft.VisualStudio.TestTools.UnitTesting;
using KBKA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBKA.Tests
{
    /// <summary>
    /// Test Class for MainWindow.xaml.cs
    /// </summary>
    [TestClass()]
    public class MainWindowTests
    {

        /// <summary>
        /// Checks if there is free port avilable and if it is returned
        /// </summary>
        [TestMethod()]

        public void GetRandomUnusedPortTestNull()
        {
            int? expected = null;
            var port = KBKA.MainWindow.GetRandomUnusedPort();
            Assert.AreNotEqual(expected, port);

        }

        /// <summary>
        /// Checks if encoded content is returned
        /// </summary>
        [TestMethod()]
        public void base64urlencodeNoPaddingTestNull()
        {
            string expected = null;
            byte[] bytes = new byte[32];
            string base64 = KBKA.MainWindow.base64urlencodeNoPadding(bytes);
            Assert.AreNotEqual(expected, base64);
        }

        /// <summary>
        /// Checks if URI-safe data is returned
        /// </summary>
        [TestMethod()]
        public void randomDataBase64urlTestNull()
        {
            string expected = null;
            string base64url = KBKA.MainWindow.randomDataBase64url(32);
            Assert.AreNotEqual(expected, base64url);
        }

        /// <summary>
        /// Checks if  the SHA256 hash of the input string is returned
        /// </summary>
        [TestMethod()]
        public void sha256TestNull()
        {
            
            byte[] expected = null;
            string code_verifier = KBKA.MainWindow.randomDataBase64url(32);
            byte[] bytes = KBKA.MainWindow.sha256(code_verifier);
            Assert.AreNotEqual(expected, bytes);
        }
    }
}