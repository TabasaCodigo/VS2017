﻿//**********************************************************************************
//
//OpenSSLKey
// .NET 2.0  OpenSSL Public & Private Key Parser
//
// Copyright (C) 2008  	JavaScience Consulting
//
//***********************************************************************************
//
//  opensslkey.cs
//
//  Reads and parses:
//    (1) OpenSSL PEM or DER public keys
//    (2) OpenSSL PEM or DER traditional SSLeay private keys (encrypted and unencrypted)
//    (3) PKCS #8 PEM or DER encoded private keys (encrypted and unencrypted)
//  Keys in PEM format must have headers/footers .
//  Encrypted Private Key in SSLEay format not supported in DER
//  Removes header/footer lines.
//  For traditional SSLEAY PEM private keys, checks for encrypted format and
//  uses PBE to extract 3DES key.
//  For SSLEAY format, only supports encryption format: DES-EDE3-CBC
//  For PKCS #8, only supports PKCS#5 v2.0  3des.
//  Parses private and public key components and returns .NET RSA object.
//  Creates dummy unsigned certificate linked to private keypair and
//  optionally exports to pkcs #12
//
// See also: 
//  http://www.openssl.org/docs/crypto/pem.html#PEM_ENCRYPTION_FORMAT 
//**************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security;
using System.Diagnostics;
using System.ComponentModel;

namespace Factunet
{
    public class Win32
    {

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertCreateSelfSignCertificate(
        IntPtr hProv,
        ref CERT_NAME_BLOB pSubjectIssuerBlob,
        uint dwFlagsm,
        ref CRYPT_KEY_PROV_INFO pKeyProvInfo,
        IntPtr pSignatureAlgorithm,
        IntPtr pStartTime,
        IntPtr pEndTime,
        IntPtr other);


        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertStrToName(
         uint dwCertEncodingType,
         String pszX500,
         uint dwStrType,
         IntPtr pvReserved,
         [In, Out] byte[] pbEncoded,
         ref uint pcbEncoded,
         IntPtr other);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertFreeCertificateContext(
        IntPtr hCertStore);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CRYPT_KEY_PROV_INFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public String pwszContainerName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public String pwszProvName;
        public uint dwProvType;
        public uint dwFlags;
        public uint cProvParam;
        public IntPtr rgProvParam;
        public uint dwKeySpec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CERT_NAME_BLOB
    {
        public int cbData;
        public IntPtr pbData;
    }


    public static class OpenSSL
    {
        const String pemprivheader = "-----BEGIN RSA PRIVATE KEY-----";
        const String pemprivfooter = "-----END RSA PRIVATE KEY-----";
        const String pempubheader = "-----BEGIN PUBLIC KEY-----";
        const String pempubfooter = "-----END PUBLIC KEY-----";
        const String pemp8header = "-----BEGIN PRIVATE KEY-----";
        const String pemp8footer = "-----END PRIVATE KEY-----";
        const String pemp8encheader = "-----BEGIN ENCRYPTED PRIVATE KEY-----";
        const String pemp8encfooter = "-----END ENCRYPTED PRIVATE KEY-----";

        // static byte[] pempublickey;
        // static byte[] pemprivatekey;
        // static byte[] pkcs8privatekey;
        // static byte[] pkcs8encprivatekey;

        public static RSACryptoServiceProvider GetRsaProviderFromPem(string pemtext)
        {
			pemtext = pemtext.Trim(new char[] { '\r', '\n', ' ' });

            byte[] pempublickey;
            byte[] pemprivatekey;
            byte[] pkcs8privatekey;
            //byte[] pkcs8encprivatekey;

            #region PEM Public Key
            if (pemtext.StartsWith(pempubheader) && pemtext.EndsWith(pempubfooter))
            {
                pempublickey = DecodeOpenSSLPublicKey(pemtext);
                if (pempublickey != null)
                {
                    //PutFileBytes("rsapubkey.pem", pempublickey, pempublickey.Length) ;
                    RSACryptoServiceProvider rsa = DecodeX509PublicKey(pempublickey);
                    Debug.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                    String xmlpublickey = rsa.ToXmlString(false);
                    Debug.WriteLine(String.Format("\nXML RSA public key:  {0} bits\n{1}\n", rsa.KeySize, xmlpublickey));
                    return rsa;
                }
                else
                {
                    Debug.WriteLine("Could not decode public key");
                    return null;
                }
            }
            #endregion
            #region PEM Private Key
            else if (pemtext.StartsWith(pemprivheader) && pemtext.EndsWith(pemprivfooter))
            {
                pemprivatekey = DecodeOpenSSLPrivateKey(pemtext);
                if (pemprivatekey != null)
                {
                    //PutFileBytes("rsaprivkey.pem", pemprivatekey, pemprivatekey.Length) ;
                    RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(pemprivatekey);
                    Debug.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                    String xmlprivatekey = rsa.ToXmlString(true);
                    Debug.WriteLine(String.Format("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey));
                    return rsa;
                }
                else
                {
                    Debug.WriteLine("Could not decode private key");
                    return null;
                }
            }
            #endregion
            #region PEM PKCS#8 PrivateKeyInfo
            else if (pemtext.StartsWith(pemp8header) && pemtext.EndsWith(pemp8footer))
            {
                pkcs8privatekey = DecodePkcs8PrivateKey(pemtext);
                if (pkcs8privatekey != null)
                {
                    //PutFileBytes("PrivateKeyInfo", pkcs8privatekey, pkcs8privatekey.Length) ;
                    RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                    if (rsa != null)
                    {
                        Debug.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                        String xmlprivatekey = rsa.ToXmlString(true);
                        Debug.WriteLine(String.Format("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey));
                        return rsa;
                    }
                    else
                    {
                        Debug.WriteLine("\nFailed to create an RSACryptoServiceProvider");
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Could not decode PKCS8 private key");
                    return null;
                }
            }
            #endregion
            else
            {
                Debug.WriteLine("Not a PEM public, private key or a PKCS #8");
                return null;
            }
        }

        //--------   Get the binary PKCS #8 PRIVATE key   --------
        public static byte[] DecodePkcs8PrivateKey(String instr)
        {
            const String pemp8header = "-----BEGIN PRIVATE KEY-----";
            const String pemp8footer = "-----END PRIVATE KEY-----";
            String pemtext = instr.Trim();
            byte[] binkey;
            if (!pemtext.StartsWith(pemp8header) || !pemtext.EndsWith(pemp8footer))
                return null;
            StringBuilder sb = new StringBuilder(pemtext);
            sb.Replace(pemp8header, "");  //remove headers/footers, if present
            sb.Replace(pemp8footer, "");

            String pubstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {
                binkey = Convert.FromBase64String(pubstr);
            }
            catch (System.FormatException)
            {		//if can't b64 decode, data is not valid
                return null;
            }
            return binkey;
        }


        //------- Parses binary asn.1 PKCS #8 PrivateKeyInfo; returns RSACryptoServiceProvider ---
        public static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            // this byte[] includes the sequence byte and terminal encoded null 
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;


                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);		//read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)	//expect an Octet string 
                    return null;

                bt = binr.ReadByte();		//read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                        binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key

                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }

            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }

        }

        //  ------  Uses PBKD2 to derive a 3DES key and decrypts data --------
        public static byte[] DecryptPBDK2(byte[] edata, byte[] salt, byte[] IV, SecureString secpswd, int iterations)
        {
            CryptoStream decrypt = null;

            IntPtr unmanagedPswd = IntPtr.Zero;
            byte[] psbytes = new byte[secpswd.Length];
            unmanagedPswd = Marshal.SecureStringToGlobalAllocAnsi(secpswd);
            Marshal.Copy(unmanagedPswd, psbytes, 0, psbytes.Length);
            Marshal.ZeroFreeGlobalAllocAnsi(unmanagedPswd);

            try
            {
                Rfc2898DeriveBytes kd = new Rfc2898DeriveBytes(psbytes, salt, iterations);
                TripleDES decAlg = TripleDES.Create();
                decAlg.Key = kd.GetBytes(24);
                decAlg.IV = IV;
                MemoryStream memstr = new MemoryStream();
                decrypt = new CryptoStream(memstr, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(edata, 0, edata.Length);
                decrypt.Flush();
                decrypt.Close();	// this is REQUIRED.
                byte[] cleartext = memstr.ToArray();
                return cleartext;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Problem decrypting: {0}", e.Message);
                return null;
            }
        }

        //--------   Get the binary RSA PUBLIC key   --------
        public static byte[] DecodeOpenSSLPublicKey(String instr)
        {
            const String pempubheader = "-----BEGIN PUBLIC KEY-----";
            const String pempubfooter = "-----END PUBLIC KEY-----";
            String pemtext = instr.Trim();
            byte[] binkey;
            if (!pemtext.StartsWith(pempubheader) || !pemtext.EndsWith(pempubfooter))
                return null;
            StringBuilder sb = new StringBuilder(pemtext);
            sb.Replace(pempubheader, "");  //remove headers/footers, if present
            sb.Replace(pempubfooter, "");

            String pubstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {
                binkey = Convert.FromBase64String(pubstr);
            }
            catch (System.FormatException)
            {		//if can't b64 decode, data is not valid
                return null;
            }
            return binkey;
        }

        //------- Parses binary asn.1 X509 SubjectPublicKeyInfo; returns RSACryptoServiceProvider ---
        public static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(x509key);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                seq = binr.ReadBytes(15);		//read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103)	//data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x00)		//expect null byte next
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102)	//data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();	// read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte();	//advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {	//if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();	//skip this null byte
                    modsize -= 1;	//reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);	//read the modulus bytes

                if (binr.ReadByte() != 0x02)			//expect an Integer for the exponent data
                    return null;
                int expbytes = (int)binr.ReadByte();		// should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);


                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAKeyInfo = new RSAParameters();
                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;
                RSA.ImportParameters(RSAKeyInfo);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }

        }

        //------- Parses binary ans.1 RSA private key; returns RSACryptoServiceProvider  ---
        public static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)	//version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------  all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Close(); }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)		//expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();	// data size in next byte
            else
                if (bt == 0x82)
                {
                    highbyte = binr.ReadByte();	// data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;		// we already have the data size
                }



            while (binr.ReadByte() == 0x00)
            {	//remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);		//last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        //-----  Get the binary RSA PRIVATE key, decrypting if necessary ----
        public static byte[] DecodeOpenSSLPrivateKey(String instr)
        {
            const String pemprivheader = "-----BEGIN RSA PRIVATE KEY-----";
            const String pemprivfooter = "-----END RSA PRIVATE KEY-----";
            String pemtext = instr.Trim();
            byte[] binkey;
            if (!pemtext.StartsWith(pemprivheader) || !pemtext.EndsWith(pemprivfooter))
                return null;

            StringBuilder sb = new StringBuilder(pemtext);
            sb.Replace(pemprivheader, "");  //remove headers/footers, if present
            sb.Replace(pemprivfooter, "");

            String pvkstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {        // if there are no PEM encryption info lines, this is an UNencrypted PEM private key
                binkey = Convert.FromBase64String(pvkstr);
                return binkey;
            }
            catch (System.FormatException)
            {	//if can't b64 decode, it must be an encrypted private key
                Debug.WriteLine("Not an unencrypted OpenSSL PEM private key");
                return null;
            }
        }

        //-----   OpenSSL PBKD uses only one hash cycle (count); miter is number of iterations required to build sufficient bytes ---
        private static byte[] GetOpenSSL3deskey(byte[] salt, SecureString secpswd, int count, int miter)
        {
            IntPtr unmanagedPswd = IntPtr.Zero;
            int HASHLENGTH = 16;	//MD5 bytes
            byte[] keymaterial = new byte[HASHLENGTH * miter];     //to store contatenated Mi hashed results


            byte[] psbytes = new byte[secpswd.Length];
            unmanagedPswd = Marshal.SecureStringToGlobalAllocAnsi(secpswd);
            Marshal.Copy(unmanagedPswd, psbytes, 0, psbytes.Length);
            Marshal.ZeroFreeGlobalAllocAnsi(unmanagedPswd);

            //UTF8Encoding utf8 = new UTF8Encoding();
            //byte[] psbytes = utf8.GetBytes(pswd);

            // --- contatenate salt and pswd bytes into fixed data array ---
            byte[] data00 = new byte[psbytes.Length + salt.Length];
            Array.Copy(psbytes, data00, psbytes.Length);		//copy the pswd bytes
            Array.Copy(salt, 0, data00, psbytes.Length, salt.Length);	//concatenate the salt bytes

            // ---- do multi-hashing and contatenate results  D1, D2 ...  into keymaterial bytes ----
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = null;
            byte[] hashtarget = new byte[HASHLENGTH + data00.Length];   //fixed length initial hashtarget

            for (int j = 0; j < miter; j++)
            {
                // ----  Now hash consecutively for count times ------
                if (j == 0)
                    result = data00;   	//initialize 
                else
                {
                    Array.Copy(result, hashtarget, result.Length);
                    Array.Copy(data00, 0, hashtarget, result.Length, data00.Length);
                    result = hashtarget;
                    //Debug.WriteLine("Updated new initial hash target:") ;
                    //showBytes(result) ;
                }

                for (int i = 0; i < count; i++)
                    result = md5.ComputeHash(result);
                Array.Copy(result, 0, keymaterial, j * HASHLENGTH, result.Length);  //contatenate to keymaterial
            }
            //showBytes("Final key material", keymaterial);
            byte[] deskey = new byte[24];
            Array.Copy(keymaterial, deskey, deskey.Length);

            Array.Clear(psbytes, 0, psbytes.Length);
            Array.Clear(data00, 0, data00.Length);
            Array.Clear(result, 0, result.Length);
            Array.Clear(hashtarget, 0, hashtarget.Length);
            Array.Clear(keymaterial, 0, keymaterial.Length);

            return deskey;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static void showWin32Error(int errorcode)
        {
            Win32Exception myEx = new Win32Exception(errorcode);
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.WriteLine(String.Format("Error code:\t 0x{0:X}", myEx.ErrorCode));
            Debug.WriteLine(String.Format("Error message:\t {0}\n", myEx.Message));
            Console.ForegroundColor = ConsoleColor.Gray;
        }


    }
}