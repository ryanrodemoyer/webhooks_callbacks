using System;
using System.Threading.Tasks;
using bizlogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;

namespace tests.netframework
{
    [TestClass, Isolated]
    public class SecurityHelperTests
    {
        [TestMethod]
        public void PlainText_Hash_ToBase64_Validation()
        {
            var secret = Isolate.Fake.Instance<ISecretRetrieverAsync>();
            Isolate.WhenCalled(() => secret.GetSecretAsync()).WillReturn(Task.FromResult("TestingSecret898989"));

            var hasher = new HMACSha256Hasher();
            string result = hasher.GenerateHash("Hello World!!", secret, new Base64BinaryFormatter());

            Assert.AreEqual("+6ArqTF24x2oANI5KUVzFx2hNdN/8kWNNOAyAbfbOoI=", result);
        }

        [TestMethod]
        public void Test02()
        {
            var secret = Isolate.Fake.Instance<ISecretRetrieverAsync>();
            Isolate.WhenCalled(() => secret.GetSecretAsync()).WillReturn(Task.FromResult("Rock'em Sock'em Robots"));

            var hasher = new HMACSha256Hasher();
            string result = hasher.GenerateHash("", secret, new Base64BinaryFormatter());

            Assert.AreEqual("MPasJm2F+OAGA1lugQAnMnGsFk362fhgD0MMG0pvczg=", result);
        }


        [TestMethod]
        public void Test05()
        {
            var secret = Isolate.Fake.Instance<ISecretRetrieverAsync>();
            Isolate.WhenCalled(() => secret.GetSecretAsync()).WillReturn(Task.FromResult(""));

            var hasher = new HMACSha256Hasher();
            string result = hasher.GenerateHash("my non-secret message to verify", secret, new Base64BinaryFormatter());

            Assert.AreEqual("+Gq6DTmAadzAj0BqJ1qExpOhlo+cMkg0rjzxMyYmTXI=", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test03()
        {
            var secret = Isolate.Fake.Instance<ISecretRetrieverAsync>();
            Isolate.WhenCalled(() => secret.GetSecretAsync()).WillReturn(Task.FromResult("TestingSecret898989"));

            var hasher = new HMACSha256Hasher();
            string result = hasher.GenerateHash(null, secret, new Base64BinaryFormatter());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test04()
        {
            var secret = Isolate.Fake.Instance<ISecretRetrieverAsync>();
            Isolate.WhenCalled(() => secret.GetSecretAsync()).WillReturn(null);

            var hasher = new HMACSha256Hasher();
            string result = hasher.GenerateHash("valid value", secret, new Base64BinaryFormatter());
        }
    }
}
