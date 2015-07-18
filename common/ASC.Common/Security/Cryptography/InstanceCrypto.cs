/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


#region usings

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace ASC.Security.Cryptography
{
    public static class InstanceCrypto
    {
        public static string Encrypt(string data)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data)));
        }

        public static byte[] Encrypt(byte[] data)
        {
            Rijndael hasher = Rijndael.Create();
            hasher.Key = EKey();
            hasher.IV = new byte[hasher.BlockSize >> 3];
            using (var ms = new MemoryStream())
            using (var ss = new CryptoStream(ms, hasher.CreateEncryptor(), CryptoStreamMode.Write))
            {
                ss.Write(data, 0, data.Length);
                ss.FlushFinalBlock();
                hasher.Clear();
                return ms.ToArray();
            }
        }

        public static string Decrypt(string data)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(data)));
        }

        public static byte[] Decrypt(byte[] data)
        {
            Rijndael hasher = Rijndael.Create();
            hasher.Key = EKey();
            hasher.IV = new byte[hasher.BlockSize >> 3];

            using (var ms = new MemoryStream(data))
            using (var ss = new CryptoStream(ms, hasher.CreateDecryptor(), CryptoStreamMode.Read))
            {
                var buffer = new byte[data.Length];
                int size = ss.Read(buffer, 0, buffer.Length);
                hasher.Clear();
                var newBuffer = new byte[size];
                Array.Copy(buffer, newBuffer, size);
                return newBuffer;
            }
        }

        private static byte[] EKey()
        {
            return MachinePseudoKeys.GetMachineConstant(32);
        }
    }
}