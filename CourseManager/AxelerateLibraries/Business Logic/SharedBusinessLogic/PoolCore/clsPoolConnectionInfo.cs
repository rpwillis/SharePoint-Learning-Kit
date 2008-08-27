using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.PoolCore
{
    public class clsPoolConnectionInfo
    {
        #region "Properties and Methods"
        public static string GetHash(string[] values)
        {
            MD5 idHash = MD5.Create();
            string valuesJoined = String.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            int i = 0;

            foreach (string val in values)
                valuesJoined += val;

            Byte[] data = idHash.ComputeHash(Encoding.Default.GetBytes(valuesJoined));

            for (i = 0; i < data.Length; i++)
                stringBuilder.Append(data[i].ToString("x2"));

            return stringBuilder.ToString();
        }

        public static int GetRandomSleep(int baseValue, int retryValue)
        {
            System.Random random = new Random(System.Environment.TickCount);
            int maxValue = System.Convert.ToInt32(Math.Pow(System.Convert.ToDouble(baseValue), System.Convert.ToDouble(retryValue)));
            return random.Next((maxValue/2), maxValue);
        }

        #endregion

        public virtual string key
        {
            get 
            {
                return string.Empty;
            }
        }
    }
}