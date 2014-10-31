using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;

namespace MTApiService
{
    public class MtRegistryManager
    {
        private const string SOFTWARE = "Software";
        private const string APP_NAME = "MtApi";
        private const string PROFILES_REGKEY = "ConnectionProfiles";
        private const string HOST_REGVALUE_NAME = "Host";
        private const string PORT_REGVALUE_NAME = "Port";
        private const string SIGNATURE_REGVALUE_NAME = "MtSignature";

        #region Public Methods
        public static IEnumerable<MtConnectionProfile> LoadConnectionProfiles()
        {
            return LoadConnectionProfilesFromRegisty();
        }

        public static MtConnectionProfile LoadConnectionProfile(string profileName)
        {
            return string.IsNullOrEmpty(profileName) == false ? LoadConnectionProfileFromRegisty(profileName) : null;
        }

        public static void AddConnectionProfile(MtConnectionProfile profile)
        {            
            if (profile != null && string.IsNullOrEmpty(profile.Name) == false)
            {
                SaveConnectionProfileToRegistry(profile);
            }
        }

        public static void RemoveConnectionProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName) == false)
            {
                DeleteConnectionProfileFromRegistry(profileName);
            }
        }

        public static string ReadSignatureKey(string accountName, string accountNumber)
        {
            if (string.IsNullOrEmpty(accountName)
                || string.IsNullOrEmpty(accountNumber))
            {
                return null;
            }

            string signature = null;

            var softwareRegKey = Registry.CurrentUser.OpenSubKey(SOFTWARE, true);
            if (softwareRegKey != null)
            {
                using (softwareRegKey)
                {
                    var appRegKey = softwareRegKey.OpenSubKey(APP_NAME, true);
                    if (appRegKey != null)
                    {
                        using (appRegKey)
                        {
                            var accountKey = appRegKey.OpenSubKey(accountName, true);
                            if (accountKey != null)
                            {
                                using (accountKey)
                                {
                                    var numberKey = accountKey.OpenSubKey(accountNumber, true);
                                    if (numberKey != null)
                                    {
                                        using (numberKey)
                                        {
                                            signature = numberKey.GetValue(SIGNATURE_REGVALUE_NAME).ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return signature;
        }

        public static string SaveSignatureKey(string accountName, string accountNumber, string signature)
        {
            if (string.IsNullOrEmpty(accountName)
                || string.IsNullOrEmpty(accountNumber))
            {
                return null;
            }

            RegistryKey softwareRgKey = Registry.CurrentUser.OpenSubKey(SOFTWARE, true);

            if (softwareRgKey == null)
                return null;

            string retVal = null;

            using (softwareRgKey)
            {

                //app name
                var appRegKey = softwareRgKey.OpenSubKey(APP_NAME, true);
                if (appRegKey == null)
                {
                    appRegKey = softwareRgKey.CreateSubKey(APP_NAME);
                }

                using (appRegKey)
                {
                    //account name
                    var accountKey = appRegKey.OpenSubKey(accountName, true);
                    if (accountKey == null)
                    {
                        accountKey = appRegKey.CreateSubKey(accountName);
                    }

                    using (accountKey)
                    {
                        //account number
                        var numberKey = accountKey.OpenSubKey(accountNumber, true);
                        if (numberKey == null)
                        {
                            numberKey = accountKey.CreateSubKey(accountNumber);
                        }

                        using (numberKey)
                        {
                            numberKey.SetValue(SIGNATURE_REGVALUE_NAME, signature);

                            retVal = numberKey.ToString();
                        }
                    }
                }
            }

            return retVal;
        }

        public static bool ExportKey(string RegKey, string SavePath)
        {
            string path = "\"" + SavePath + "\"";
            string key = "\"" + RegKey + "\"";

            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "regedit.exe";
                proc.StartInfo.UseShellExecute = false;
                proc = Process.Start("regedit.exe", "/e " + path + " " + key + "");

                if (proc != null) 
                    proc.WaitForExit();
            }
            catch(Exception)
            {
                return false;
            }
            finally
            {
                if (proc != null) 
                    proc.Dispose();
            }

            return true;
        }

        #endregion

        #region Private Methods
        private static IEnumerable<MtConnectionProfile> LoadConnectionProfilesFromRegisty()
        {
            List<MtConnectionProfile> profiles = null;

            var softwareRegKey = Registry.CurrentUser.OpenSubKey(SOFTWARE, true);
            if (softwareRegKey != null)
            {
                using (softwareRegKey)
                {
                    var appRegKey = softwareRegKey.OpenSubKey(APP_NAME, true);
                    if (appRegKey != null)
                    {
                        using (appRegKey)
                        {
                            var profilesRegKey = appRegKey.OpenSubKey(PROFILES_REGKEY, true);
                            if (profilesRegKey != null)
                            {
                                using (profilesRegKey)
                                {
                                    profiles = new List<MtConnectionProfile>();

                                    foreach (string profileNameKey in profilesRegKey.GetSubKeyNames())
                                    {
                                        using (RegistryKey tempKey = profilesRegKey.OpenSubKey(profileNameKey))
                                        {
                                            var profile = new MtConnectionProfile(profileNameKey);

                                            profile.Host = tempKey.GetValue(HOST_REGVALUE_NAME).ToString();
                                            profile.Port = (int)tempKey.GetValue(PORT_REGVALUE_NAME);

                                            profiles.Add(profile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return profiles;
        }

        private static MtConnectionProfile LoadConnectionProfileFromRegisty(string profileName)
        {
            MtConnectionProfile profile = null;

            var softwareRegKey = Registry.CurrentUser.OpenSubKey(SOFTWARE, true);
            if (softwareRegKey != null)
            {
                using (softwareRegKey)
                {
                    var appRegKey = softwareRegKey.OpenSubKey(APP_NAME, true);
                    if (appRegKey != null)
                    {
                        using (appRegKey)
                        {
                            var profilesRegKey = appRegKey.OpenSubKey(PROFILES_REGKEY, true);
                            if (profilesRegKey != null)
                            {
                                using (profilesRegKey)
                                {
                                    using (RegistryKey tempKey = profilesRegKey.OpenSubKey(profileName))
                                    {
                                        profile = new MtConnectionProfile(profileName);

                                        profile.Host = tempKey.GetValue(HOST_REGVALUE_NAME).ToString();
                                        profile.Port = (int)tempKey.GetValue(PORT_REGVALUE_NAME);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return profile;
        }

        private static void SaveConnectionProfileToRegistry(MtConnectionProfile profile)
        {
            RegistryKey softwareRgKey = Registry.CurrentUser.OpenSubKey(SOFTWARE, true);

            if (softwareRgKey == null)
                return;

            using (softwareRgKey)
            {
                //app name
                var appRegKey = softwareRgKey.OpenSubKey(APP_NAME, true);
                if (appRegKey == null)
                {
                    appRegKey = softwareRgKey.CreateSubKey(APP_NAME);
                }

                using (appRegKey)
                {
                    //ConnectionProfiles key
                    var profilesRegKey = appRegKey.OpenSubKey(PROFILES_REGKEY, true);
                    if (profilesRegKey == null)
                    {
                        profilesRegKey = appRegKey.CreateSubKey(PROFILES_REGKEY);
                    }

                    using (profilesRegKey)
                    {
                        var profileKey = profilesRegKey.CreateSubKey(profile.Name);

                        using (profileKey)
                        {
                            profileKey.SetValue(HOST_REGVALUE_NAME, profile.Host);
                            profileKey.SetValue(PORT_REGVALUE_NAME, profile.Port);
                        }
                    }
                }
            }
        }

        private static void DeleteConnectionProfileFromRegistry(string profileName)
        {
            var softwareRegKey = Registry.CurrentUser.OpenSubKey(SOFTWARE, true);
            if (softwareRegKey == null)
                return;

            using (softwareRegKey)
            {
                var appRegKey = softwareRegKey.OpenSubKey(APP_NAME, true);
                if (appRegKey == null)
                    return;

                using (appRegKey)
                {
                    var profilesRegKey = appRegKey.OpenSubKey(PROFILES_REGKEY, true);
                    if (profilesRegKey == null)
                        return;

                    using (profilesRegKey)
                    {
                        profilesRegKey.DeleteSubKey(profileName);
                    }
                }
            }
        }
        #endregion

        #region Fields

        #endregion
    }
}
