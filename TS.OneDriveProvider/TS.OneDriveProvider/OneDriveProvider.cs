using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TrailheadSystems.OneDriveProvider
{
	[CmdletProvider("OneDriveProvider", ProviderCapabilities.None)]
	public class OneDriveProvider : NavigationCmdletProvider
	{
		protected override bool IsValidPath(string path)
		{
			throw new NotImplementedException();
		}

		protected async void ForceAuth()
		{
			await oneDriveClient.AuthenticationProvider.AuthenticateRequestAsync(oneDriveClient.Drives.Request().GetHttpRequestMessage());

		}
		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			Debug.WriteLine(oneDriveClient);
			return base.NewDrive(drive);
		}
		protected async void GetDrives()
		{
			mresult = await oneDriveClient.Drives.Request().GetAsync();
		}

		private static OneDriveClient oneDriveClient;
		protected override Collection<PSDriveInfo> InitializeDefaultDrives()
		{
			IAuthenticationProvider authProvider = new DelegateAuthenticationProvider(
							async (requestMessage) =>
							{
								var token = await GetTokenForUserAsync();
								requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
								// This header has been added to identify our sample in the Microsoft Graph service.  If extracting this code for your project please remove.
								//requestMessage.Headers.Add("SampleID", "uwp-csharp-snippets-sample");

							});
			oneDriveClient = new OneDriveClient("https://graph.microsoft.com/v1.0", authProvider);


			//ForceAuth();

			GetDrives();

			var drive = new PSDriveInfo("OneDrive", this.ProviderInfo, string.Empty, "OneDrive default drive", null);

			return new Collection<PSDriveInfo>(new[] { drive });
		}
		public static async Task<string> GetTokenForUserAsync()
		{
			if (TokenForUser == null || expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
			{
				var redirectUri = new Uri(returnUrl);
				var scopes = new string[]
						{
						"https://graph.microsoft.com/User.Read",
						"https://graph.microsoft.com/User.ReadWrite",
						"https://graph.microsoft.com/User.ReadBasic.All",
						"https://graph.microsoft.com/Mail.Send",
						"https://graph.microsoft.com/Calendars.ReadWrite",
						"https://graph.microsoft.com/Mail.ReadWrite",
						"https://graph.microsoft.com/Files.ReadWrite",

                        // Admin-only scopes. Uncomment these if you're running the sample with an admin work account.
                        // You won't be able to sign in with a non-admin work account if you request these scopes.
                        // These scopes will be ignored if you leave them uncommented and run the sample with a consumer account.
                        // See the MainPage.xaml.cs file for all of the operations that won't work if you're not running the 
                        // sample with an admin work account.
                        //"https://graph.microsoft.com/Directory.AccessAsUser.All",
                        //"https://graph.microsoft.com/User.ReadWrite.All",
                        //"https://graph.microsoft.com/Group.ReadWrite.All"


                    };

				IdentityClientApp = new PublicClientApplication(clientId);
				AuthenticationResult authResult = await IdentityClientApp.AcquireTokenAsync(scopes);

				TokenForUser = authResult.Token;
				//not sure if I need Id or Access Token here
				//TokenForUser = authResult.IdToken;
				expiration = authResult.ExpiresOn;
			}

			return TokenForUser;
		}

		static string clientId = "3eeab1cf-69bb-4db0-8d19-ba16cf28f647";
		static string returnUrl = "urn:ietf:wg:oauth:2.0:oob";

		public static PublicClientApplication IdentityClientApp = null;
		public static string TokenForUser = null;
		public static DateTimeOffset expiration;
		private static IOneDriveDrivesCollectionPage mresult;

		/// <summary>
		/// Signs the user out of the service.
		/// </summary>
		public static void SignOut()
		{
			foreach (var user in IdentityClientApp.Users)
			{
				//not sure how to sign out after Identity.Client changes
				user.SignOut();
			}

			TokenForUser = null;

		}
	}
}
