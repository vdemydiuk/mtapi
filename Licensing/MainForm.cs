using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security;
using MTApiService;

namespace Licensing
{
    public partial class MainForm : Form
    {
        private bool mIsSaved = true;

        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonGenerateKey_Click(object sender, EventArgs e)
        {
            textBoxPrivateKey.Text = DigitalSignatureHelper.GeneratePrivateKey();

            mIsSaved = false;
        }

        private void buttonGetPublicKey_Click(object sender, EventArgs e)
        {
            string privateKey = textBoxPrivateKey.Text;

            if (string.IsNullOrEmpty(privateKey) == false)
            {
                textBoxPublicKey.Text = DigitalSignatureHelper.GetPublicKey(privateKey);
            }
            else
            {
                MessageBox.Show("Private Key is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonSaveKey_Click(object sender, EventArgs e)
        {
            string privateKey = textBoxPrivateKey.Text;

            if (string.IsNullOrEmpty(privateKey) == false)
            {
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Filter = "MetaTraderApi Key Files (*.mtk)|*.mtk";
                dlg.Title = "Save MetaTraderApi Key File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (TextWriter file = new StreamWriter(dlg.FileName, true))
                        {
                            file.WriteLine(privateKey);
                        }

                        mIsSaved = true;
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = string.Format("Saving failed!\n{0}", ex.Message);
                        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Private Key is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonLoadKey_Click(object sender, EventArgs e)
        {
            if (mIsSaved == false)
            {
                if (MessageBox.Show("Private Key is not saved. Continue?", "Private Key", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;
            }

             OpenFileDialog dlg = new OpenFileDialog();

             dlg.Filter = "MetaTraderApi Key Files (*.mtk)|*.mtk";
             dlg.Title = "Load MetaTraderApi Key File";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxPrivateKey.Text = string.Empty;
                textBoxPublicKey.Text = string.Empty;

                try
                {
                    using (TextReader file = new StreamReader(dlg.FileName))
                    {
                        textBoxPrivateKey.Text = file.ReadLine();
                    }

                    mIsSaved = true;
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format("Saving failed!\n{0}", ex.Message);
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonSign_Click(object sender, EventArgs e)
        {
            string privateKey = textBoxPrivateKey.Text;

            if (string.IsNullOrEmpty(privateKey) == false)
            {
                string accountName = textBoxAccountName.Text;
                string accountNumber = textBoxAccountNumber.Text;

                if (string.IsNullOrEmpty(accountName) == false)
                {
                    if (string.IsNullOrEmpty(accountNumber) == false)
                    {
                        string inputData = accountName + accountNumber;

                        textBoxSignature.Text = DigitalSignatureHelper.CreateSignature(inputData, privateKey);
                    }
                    else
                    {
                        MessageBox.Show("AccountNumber is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("AccountName is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Private Key is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonExportPublicKey_Click(object sender, EventArgs e)
        {
            string publicKey = textBoxPublicKey.Text;

            if (string.IsNullOrEmpty(publicKey) == false)
            {
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Filter = "MetaTraderApi Key Files (*.mtk)|*.mtk";
                dlg.Title = "Export MetaTraderApi Key File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (TextWriter file = new StreamWriter(dlg.FileName, true))
                        {
                            file.WriteLine(publicKey);
                        }

                        mIsSaved = true;
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = string.Format("Export failed!\n{0}", ex.Message);
                        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Public Key is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonExportSignature_Click(object sender, EventArgs e)
        {
            string signature = textBoxSignature.Text;

            if (string.IsNullOrEmpty(signature) == false)
            {
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Filter = "MetaTraderApi Key (*.mta)|*.mta";
                dlg.Title = "Export MetaTraderApi Key";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (TextWriter file = new StreamWriter(dlg.FileName))
                        {
                            file.WriteLine(signature);
                        }

                        mIsSaved = true;
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = string.Format("Export failed!\n{0}", ex.Message);
                        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Signature is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonVerify_Click(object sender, EventArgs e)
        {
            string publicKey = textBoxPublicKey.Text;

            if (string.IsNullOrEmpty(publicKey) == false)
            {
                string signature = textBoxSignature.Text;

                if (string.IsNullOrEmpty(signature) == false)
                {
                    string accountName = textBoxAccountName.Text;
                    string accountNumber = textBoxAccountNumber.Text;

                    if (string.IsNullOrEmpty(accountName) == false)
                    {
                        if (string.IsNullOrEmpty(accountNumber) == false)
                        {
                            string inputData = accountName + accountNumber;
                            var verifyResult = false;
                            try
                            {
                                verifyResult = DigitalSignatureHelper.VerifySignature(inputData, signature, publicKey);
                            }
                            catch (SecurityException ex)
                            {
                                MessageBox.Show(ex.Message, "Verify", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                verifyResult = false;
                            }
                            catch (Exception ex)
                            {
                                string errorMsg = string.Format("Verify process failed!\n{0}", ex.Message);
                                MessageBox.Show(errorMsg, "Verify", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                verifyResult = false;
                            }

                            if (verifyResult == true)
                            {
                                MessageBox.Show("Signature good", "Verify", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                            }                            
                        }
                        else
                        {
                            MessageBox.Show("AccountNumber is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("AccountName is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                else
                {
                    MessageBox.Show("Signature is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Public Key is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void clipboardCopyBtn_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxPublicKey.Text);
        }

        private void saveToRegBtn_Click(object sender, EventArgs e)
        {
            string signature = textBoxSignature.Text;
            string accountName = textBoxAccountName.Text;
            string accountNumber = textBoxAccountNumber.Text;

            if (string.IsNullOrEmpty(signature))
            {
                MessageBox.Show("Signature is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(accountName))
            {
                MessageBox.Show("AccountName is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(accountNumber))
            {
                MessageBox.Show("AccountNumber is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "Registry files (*.reg)|*.reg";
            dlg.Title = "Registry files";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string key = MtRegistryManager.SaveSignatureKey(accountName, accountNumber, signature);

                bool exported = MtRegistryManager.ExportKey(key, dlg.FileName);

                if (exported)
                {
                    MessageBox.Show("Export successed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    MessageBox.Show("Export failed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void readRegBtn_Click(object sender, EventArgs e)
        {
            string accountName = textBoxAccountName.Text;
            string accountNumber = textBoxAccountNumber.Text;

            if (string.IsNullOrEmpty(accountName))
            {
                MessageBox.Show("AccountName is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(accountNumber))
            {
                MessageBox.Show("AccountNumber is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string signature = MtRegistryManager.ReadSignatureKey(accountName, accountNumber);
            textBoxSignature.Text = signature;
        }
    }
}
