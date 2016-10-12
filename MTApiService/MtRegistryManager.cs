using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;

namespace MTApiService
{
    public class MtRegistryManager
    {
        private const string Software = "Software";
        private const string AppName = "MtApi";
        private const string ProfilesRegkey = "ConnectionProfiles";
        private const string HostRegvalueName = "Host";
        private const string PortRegvalueName = "Port";
        private const string SignatureRegvalueName = "MtSignature";

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

            var softwareRegKey = Registry.CurrentUser.OpenSubKey(Software, true);
            if (softwareRegKey != null)
            {
                using (softwareRegKey)
                {
                    var appRegKey = softwareRegKey.OpenSubKey(AppName, true);
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
                                            signature = numberKey.GetValue(SignatureRegvalueName).ToString();
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

            var softwareRgKey = Registry.CurrentUser.OpenSubKey(Software, true);

            if (softwareRgKey == null)
                return null;

            string retVal = null;

            using (softwareRgKey)
            {

                //app name
                var appRegKey = softwareRgKey.OpenSubKey(AppName, true) ?? softwareRgKey.CreateSubKey(AppName);

                if (appRegKey != null)
                {
                    using (appRegKey)
                    {
                        //account name
                        var accountKey = appRegKey.OpenSubKey(accountName, true) ?? appRegKey.CreateSubKey(accountName);

                        if (accountKey != null)
                        {
                            using (accountKey)
                            {
                                //account number
                                var numberKey = accountKey.OpenSubKey(accountNumber, true) ??
                                                accountKey.CreateSubKey(accountNumber);

                                if (numberKey != null)
                                {
                                    using (numberKey)
                                    {
                                        numberKey.SetValue(SignatureRegvalueName, signature);

                                        retVal = numberKey.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return retVal;
        }

        public static bool ExportKey(string regKey, string savePath)
        {
            var path = $"\"{savePath}\"";
            var key = $"\"{regKey}\"";

            var proc = new Process();
            try
            {
                proc.StartInfo.FileName = "regedit.exe";
                proc.StartInfo.UseShellExecute = false;
                proc = Process.Start("regedit.exe", "/e " + path + " " + key + "");

                proc?.WaitForExit();
            }
            catch(Exception)
            {
                return false;
            }
            finally
            {
                proc?.Dispose();
            }

            return true;
        }

        #endregion

        #region Private Methods
        private static IEnumerable<MtConnectionProfile> LoadConnectionProfilesFromRegisty()
        {
            List<MtConnectionProfile> profiles = null;

            var softwareRegKey = Registry.CurrentUser.OpenSubKey(Software, true);
            if (softwareRegKey != null)
            {
                using (softwareRegKey)
                {
                    var appRegKey = softwareRegKey.OpenSubKey(AppName, true);
                    if (appRegKey != null)
                    {
                        using (appRegKey)
                        {
                            var profilesRegKey = appRegKey.OpenSubKey(ProfilesRegkey, true);
                            if (profilesRegKey != null)
                            {
                                using (profilesRegKey)
                                {
                                    profiles = new List<MtConnectionProfile>();

                                    foreach (var profileNameKey in profilesRegKey.GetSubKeyNames())
                                    {
                                        var tempKey = profilesRegKey.OpenSubKey(profileNameKey);
                                        if (tempKey != null)
                                        {
                                            using (tempKey)
                                            {
                                                var profile = new MtConnectionProfile(profileNameKey)
                                                {
                                                    Host = tempKey.GetValue(HostRegvalueName).ToString(),
                                                    Port = (int) tempKey.GetValue(PortRegvalueName)
                                                };

                                                profiles.Add(profile);
                                            }
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

            var softwareRegKey = Registry.CurrentUser.OpenSubKey(Software, true);
            if (softwareRegKey != null)
            {
                using (softwareRegKey)
                {
                    var appRegKey = softwareRegKey.OpenSubKey(AppName, true);
                    if (appRegKey != null)
                    {
                        using (appRegKey)
                        {
                            var profilesRegKey = appRegKey.OpenSubKey(ProfilesRegkey, true);
                            if (profilesRegKey != null)
                            {
                                using (profilesRegKey)
                                {
                                    var tempKey = profilesRegKey.OpenSubKey(profileName);
                                    if (tempKey != null)
                                    {
                                        using (tempKey)
                                        {
                                            profile = new MtConnectionProfile(profileName)
                                            {
                                                Host = tempKey.GetValue(HostRegvalueName).ToString(),
                                                Port = (int) tempKey.GetValue(PortRegvalueName)
                                            };

                                        }
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
            var softwareRgKey = Registry.CurrentUser.OpenSubKey(Software, true);

            if (softwareRgKey == null)
                return;

            using (softwareRgKey)
            {
                //app name
                var appRegKey = softwareRgKey.OpenSubKey(AppName, true) ?? softwareRgKey.CreateSubKey(AppName);

                if (appRegKey != null)
                {
                    using (appRegKey)
                    {
                        //ConnectionProfiles key
                        var profilesRegKey = appRegKey.OpenSubKey(ProfilesRegkey, true) ??
                                             appRegKey.CreateSubKey(ProfilesRegkey);

                        if (profilesRegKey != null)
                        {
                            using (profilesRegKey)
                            {
                                var profileKey = profilesRegKey.CreateSubKey(profile.Name);
                                if (profileKey != null)
                                {
                                    using (profileKey)
                                    {
                                        profileKey.SetValue(HostRegvalueName, profile.Host);
                                        profileKey.SetValue(PortRegvalueName, profile.Port);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DeleteConnectionProfileFromRegistry(string profileName)
        {
            var softwareRegKey = Registry.CurrentUser.OpenSubKey(Software, true);
            if (softwareRegKey == null)
                return;

            using (softwareRegKey)
            {
                var appRegKey = softwareRegKey.OpenSubKey(AppName, true);
                if (appRegKey == null)
                    return;

                using (appRegKey)
                {
                    var profilesRegKey = appRegKey.OpenSubKey(ProfilesRegkey, true);
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
