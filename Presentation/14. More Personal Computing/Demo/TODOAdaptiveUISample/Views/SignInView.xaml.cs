using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Common;
using TODOAdaptiveUISample.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TODOAdaptiveUISample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignInView : Page
    {
        private bool m_addingAccount = true;
        private Account m_account;
        private bool m_passportAvailable = true;

        public SignInView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PassportAvailableCheck();

            if (e.Parameter != null)
            {
                m_account = (Account)e.Parameter;
                textbox_Username.Text = m_account.Email;
                textbox_Username.IsEnabled = false;
                m_addingAccount = false;

                if (m_account.UsesPassport == true)
                {
                    SignInPassport(true);
                }
            }
        }

        private void Button_SignIn_Click(object sender, RoutedEventArgs e)
        {
            SignInPassword(false);
        }

        private void Button_Passport_Click(object sender, RoutedEventArgs e)
        {
            if (m_addingAccount == false)
            {
                SignInPassport(m_account.UsesPassport);
            }
            else
            {
                SignInPassport(false);
            }
        }

        private async void PassportAvailableCheck()
        {
            var keyCredentialAvailable = await KeyCredentialManager.IsSupportedAsync();

            if (keyCredentialAvailable == false)
            {
                //
                // Key credential is not enabled yet as user 
                // needs to connect to a MSA account and select a pin in the connecting flow.
                //
                grid_PassportStatus.Background = new SolidColorBrush(Color.FromArgb(255, 50, 170, 207));
                textblock_PassportStatusText.Text = "Microsoft Passport is not set up.\nPlease go to Windows Settings and connect an MSA account!";
                button_PassportSignIn.IsEnabled = false;
                m_passportAvailable = false;
            }
        }

        private async void SignInPassport(bool passportIsPrimaryLogin)
        {
            if (passportIsPrimaryLogin == true)
            {
                if (await AuthenticatePassport() == true)
                {
                    SuccessfulSignIn(m_account);
                    return;
                }
            }
            else if (await SignInPassword(true) == true)
            {
                if (await CreatePassportKey(textbox_Username.Text) == true)
                {
                    bool serverAddedPassportToAccount = await AddPassportToAccountOnServer();

                    if (serverAddedPassportToAccount == true)
                    {
                        if (m_addingAccount == true)
                        {
                            Account goodAccount = new Account() { Name = textbox_Username.Text, Email = textbox_Username.Text, UsesPassport = true };
                            SuccessfulSignIn(goodAccount);
                        }
                        else
                        {
                            m_account.UsesPassport = true;
                            SuccessfulSignIn(m_account);
                        }
                        return;
                    }
                }
                textblock_ErrorField.Text = "Sign In with Passport failed. Try later.";
                button_PassportSignIn.IsEnabled = false;
            }
            textblock_ErrorField.Text = "Invalid username or password.";
        }

        private async Task<bool> SignInPassword(bool calledFromPassport)
        {
            textblock_ErrorField.Text = "";

            if (textbox_Username.Text.Length == 0 || passwordbox_Password.Password.Length == 0)
            {
                textblock_ErrorField.Text = "Username/Password cannot be blank.";
                return false;
            }

            try
            {
                bool signedIn = await AuthenticatePasswordCredentials();

                if (signedIn == false)
                {
                    textblock_ErrorField.Text = "Unable to sign you in with those credentials.";
                }
                else
                {
                    // TODO: Roaming Passport settings. Make it so the server can tell us if they prefer to use Passport and upsell immediately.

                    Account goodAccount = new Account() { Name = textbox_Username.Text, Email = textbox_Username.Text, UsesPassport = false };
                    if (calledFromPassport == false)
                    {
                        SuccessfulSignIn(goodAccount);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<bool> AuthenticatePasswordCredentials()
        {
            // TODO: Authenticate with server once that part is done for the sample.

            return true;
        }

        private async Task<bool> AuthenticatePassport()
        {
            IBuffer message = CryptographicBuffer.ConvertStringToBinary("LoginAuth", BinaryStringEncoding.Utf8);
            IBuffer authMessage = await GetPassportAuthenticationMessage(message, m_account.Email);

            if (authMessage != null)
            {
                return true;
            }

            return false;
        }

        private void SuccessfulSignIn(Account account)
        {
            // If this is an already existing account, replace the old
            // version of this account in the account list.
            if (m_addingAccount == false)
            {
                foreach (Account a in UserSelect.accountList)
                {
                    if (a.Email == account.Email)
                    {
                        UserSelect.accountList.Remove(a);
                        break;
                    }
                }
            }

            UserSelect.accountList.Add(account);

            CleanUpUserList();

            ((App)(Application.Current)).NavigationService.Navigate(typeof(MainPage), account.Name);

        }

        private async Task<bool> AddPassportToAccountOnServer()
        {
            // TODO: Add Passport signing info to server when that part is done for the sample

            return true;
        }

        private void CleanUpUserList()
        {
            AccountsHelper.SaveAccountList(UserSelect.accountList);
        }

        private async Task<bool> CreatePassportKey(string accountId)
        {
            KeyCredentialRetrievalResult keyCreationResult = await KeyCredentialManager.RequestCreateAsync(accountId, KeyCredentialCreationOption.ReplaceExisting);

            if (keyCreationResult.Status == KeyCredentialStatus.Success)
            {

                KeyCredential userKey = keyCreationResult.Credential;
                IBuffer publicKey = userKey.RetrievePublicKey();
                KeyCredentialAttestationResult keyAttestationResult = await userKey.GetAttestationAsync();

                IBuffer keyAttestation = null;
                IBuffer certificateChain = null;
                bool keyAttestationIncluded = false;
                bool keyAttestationCanBeRetrievedLater = false;
                KeyCredentialAttestationStatus keyAttestationRetryType = 0;

                if (keyAttestationResult.Status == KeyCredentialAttestationStatus.Success)
                {
                    keyAttestationIncluded = true;
                    keyAttestation = keyAttestationResult.AttestationBuffer;
                    certificateChain = keyAttestationResult.CertificateChainBuffer;
                }
                else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.TemporaryFailure)
                {
                    keyAttestationRetryType = KeyCredentialAttestationStatus.TemporaryFailure;
                    keyAttestationCanBeRetrievedLater = true;
                }
                else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.NotSupported)
                {
                    keyAttestationRetryType = KeyCredentialAttestationStatus.NotSupported;
                    keyAttestationCanBeRetrievedLater = false;
                }

                // Package public key, keyAttesation if available, 
                // certificate chain for attestation endorsement key if available,  
                // status code of key attestation result: keyAttestationIncluded or 
                // keyAttestationCanBeRetrievedLater and keyAttestationRetryType
                // and send it to application server to register the user.
                bool serverAddedPassportToAccount = await AddPassportToAccountOnServer();

                if (serverAddedPassportToAccount == true)
                {
                    return true;
                }
            }
            else if (keyCreationResult.Status == KeyCredentialStatus.UserCanceled)
            {
                // User cancelled the Passport enrollment process
            }
            else if (keyCreationResult.Status == KeyCredentialStatus.NotFound)
            {
                // User needs to create PIN
                textblock_PassportStatusText.Text = "Microsoft Passport is almost ready!\nPlease go to Windows Settings and set up a PIN to use it.";
                grid_PassportStatus.Background = new SolidColorBrush(Color.FromArgb(255, 50, 170, 207));
                button_PassportSignIn.IsEnabled = false;

                m_passportAvailable = false;
            }
            else
            {
            }

            return false;
        }

        private async Task<IBuffer> GetPassportAuthenticationMessage(IBuffer message, string accountId)
        {
            KeyCredentialRetrievalResult openKeyResult = await KeyCredentialManager.OpenAsync(accountId);

            if (openKeyResult.Status == KeyCredentialStatus.Success)
            {
                KeyCredential userKey = openKeyResult.Credential;
                IBuffer publicKey = userKey.RetrievePublicKey();

                KeyCredentialOperationResult signResult = await userKey.RequestSignAsync(message);

                if (signResult.Status == KeyCredentialStatus.Success)
                {
                    return signResult.Result;
                }
                else if (signResult.Status == KeyCredentialStatus.UserCanceled)
                {
                    // User cancelled the Passport PIN entry.
                    //
                    // We will return null below this and the username/password
                    // sign in form will show.
                }
                else if (signResult.Status == KeyCredentialStatus.NotFound)
                {
                    // Must recreate Passport key
                }
                else if (signResult.Status == KeyCredentialStatus.SecurityDeviceLocked)
                {
                    // Can't use Passport right now, remember that hardware failed and suggest restart
                }
                else if (signResult.Status == KeyCredentialStatus.UnknownError)
                {
                    // Can't use Passport right now, try again later
                }

                return null;
            }
            else if (openKeyResult.Status == KeyCredentialStatus.NotFound)
            {
                // Passport key lost, need to recreate it
                textblock_PassportStatusText.Text = "Microsoft Passport is almost ready!\nPlease go to Windows Settings and set up a PIN to use it.";
                grid_PassportStatus.Background = new SolidColorBrush(Color.FromArgb(255, 50, 170, 207));
                button_PassportSignIn.IsEnabled = false;

                m_passportAvailable = false;
            }
            else
            {
                // Can't use Passport right now, try again later
            }
            return null;
        }
    }
}
