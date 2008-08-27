using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

using System.Configuration;
using System.Security.Cryptography;
using System.IO;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties
{
    [Serializable(), SecurityToken("clsConfigurationProfile", "clsConfigurationProfiles", "MIPCustom")]
    public class clsConfigurationProfile : GUIDNameBusinessTemplate<clsConfigurationProfile>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsConfigurationProfile), "ConfigurationProfiles", "_cfp", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        #endregion

        #region "Business Properties and Methods"
        public clsConfigurationProfile()
        {

        }


        public static clsConfigurationProfile Current
        {
            get
            {
                String DefaultName = ConfigurationManager.AppSettings.Get("ConfigurationFile");
                if (DefaultName != null)
                {
                    clsConfigurationProfile profile = new clsConfigurationProfile();                    
                    profile.Name = DefaultName;
                    return profile;
                }
                else
                {
                    return new clsConfigurationProfile();
                }
            }
        }

        /*
        public clsConfigurationProperties getProperties()
        {
            return clsConfigurationProperties.GetCollectionByProfileName(this.Name);
        }*/

        public String getPropertyValue(String propertyName)
        {
            clsProfile_Properties properties = clsProfile_Properties.GetPropertybyProfileName(this.Name, propertyName);
            bool encrypted = false;
            String returnedValue = "";
            if (properties.Count > 0)
            {
                clsProfile_Property property = properties[0];
                returnedValue = property.Value;
                encrypted = property.Encrypted;
            }
            else
            {
                clsConfigurationProperty property = clsConfigurationProperty.GetProperty(propertyName);
                if (property == null)
                {
                    return null;
                }
                else
                {
                    returnedValue = property.DefaultValue;
                    encrypted = property.Encrypted;
                }
            }
            if (encrypted)
            {
                DESCryptoServiceProvider key = new DESCryptoServiceProvider();
                key.IV = Convert.FromBase64String(this.getPropertyValue("IV"));
                return clsConfigurationProfile.Decrypt(returnedValue, key);
            }
            else
            {
                return returnedValue;
            }
        }

        public void setProperty(String propertyName)
        {

        }

        #endregion

        #region encryption
        private static string Decrypt(String Value, SymmetricAlgorithm key)
        {
            key.Key = clsConfigurationProfile.getKey(key.Key.Length);
            byte[] buffer = Convert.FromBase64String(Value);

            // Create a memory stream to the passed buffer.
            MemoryStream ms = new MemoryStream(buffer);

            // Create a CryptoStream using the memory stream and the 
            // CSP DES key. 
            CryptoStream encStream = new CryptoStream(ms, key.CreateDecryptor(), CryptoStreamMode.Read);

            // Create a StreamReader for reading the stream.
            StreamReader sr = new StreamReader(encStream);

            // Read the stream as a string.
            string val = sr.ReadLine();

            // Close the streams.
            sr.Close();
            encStream.Close();
            ms.Close();

            return val;
        }

        private static byte[] getKey(int arraySize)
        {
            String key = ConfigurationManager.AppSettings.Get("Key");
            SHA1CryptoServiceProvider myHash = new SHA1CryptoServiceProvider();
            byte[] encrypt = Convert.FromBase64String(key);
            myHash.ComputeHash(encrypt);
            byte[] encripted = new byte[arraySize];
            for (int index = 0; index != arraySize; index++)
            {
                encripted[index] = myHash.Hash[index];
            }
            return encripted;
        }
        #endregion
    }
}
