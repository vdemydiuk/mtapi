using System; 
using System.Security.Cryptography; 
using System.Text;
using System.Security;

namespace Licensing
{
    public class DigitalSignatureHelper
    {
        public static String GeneratePrivateKey()
        {
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
            return dsa.ToXmlString(true);
        }

        public static String GetPublicKey(String privateKey)
        {
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
            dsa.FromXmlString(privateKey);
            return dsa.ToXmlString(false);
        }

        public static string CreateSignature(string inputData, String privateKey)
        {
            // create the crypto-service provider:
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();

            // setup the dsa from the private key:
            dsa.FromXmlString(privateKey);

            byte[] data = UTF8Encoding.ASCII.GetBytes(inputData);

            // get the signature:
            byte[] signature = dsa.SignData(data);

            return Convert.ToBase64String(signature);
        }

        public static bool VerifySignature(string inputData, string signature, string publicKey)
        {
            // create the crypto-service provider:
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();

            // setup the provider from the public key:
            dsa.FromXmlString(publicKey);

            // get the license terms data:
            //byte[] data = Convert.FromBase64String(inputData);
            byte[] data = UTF8Encoding.ASCII.GetBytes(inputData);

            // get the signature data:
            byte[] signatureData = Convert.FromBase64String(signature);

            // verify that the license-terms match the signature data
            if (dsa.VerifyData(data, signatureData) == false)
                throw new SecurityException("Signature Not Verified!");

            return true;
        }
    }
}
