using IssueConnectorLib.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Trimble.Identity;
using System.Net.Http.Headers;
using EasyAccessService.Models;

namespace EasyAccessService.Connector
{
	public class TrimbleEasyAccessConnector
	{

		static HttpClient client = new HttpClient();

		//TODO this should not be hard coded. 
		private const string AuthorityUri = "https://identity.trimble.com/i/oauth2/";

		public const string EasyAccessAuthUri = "https://easyaccess-tid.azurewebsites.net/";

		private static readonly ClientCredential ClientCredentials = new ClientCredential("", "", "IssueConnector");

		private static HttpClient InitializeHttpClient(EasyAccessToken token, string url)
		{
			HttpClient client = new HttpClient();

			client.BaseAddress = new Uri(url);

			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			client.DefaultRequestHeaders.Clear();

			//Use correct authorization, bearer + token
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Access_Token);

			return client;
		}

		public static async Task<HttpClient> Connect(string username, string password, string url)
		{
			var authCtx = new AuthenticationContext(ClientCredentials, new TokenCache()) { AuthorityUri = new Uri(AuthorityUri) };

			try
			{
				Debug.WriteLine("Acquiring TID token for Easy Access...");

				EasyAccessToken eaToken = await GetEasyAccessToken(authCtx, username, password);

				Debug.WriteLine("Acquired Easy Access token.");
				Debug.WriteLine("Token: " + eaToken.Access_Token);

				return InitializeHttpClient(eaToken, url);


			} catch (Exception e)
			{
				Debug.WriteLine(e.StackTrace);
				throw new EasyAccessConnectionException("Error connecting to easy access...", e);
			}

		}

		//Get the ea token from trimble identity
		private static async Task<EasyAccessToken> GetEasyAccessToken(
			AuthenticationContext authCtx, 
			string username, 
			string password)
		{
			var trimbleIdToken = await authCtx.AcquireTokenAsync(new NetworkCredential(username, password));

			var options = new Dictionary<string, string> { { "grant_type", "password" } };

			var content = new FormUrlEncodedContent(options);

			var response = await client.PostAsync(EasyAccessAuthUri + "token?idToken=" + trimbleIdToken.IdToken, content);

			return await response.Content.ReadAsAsync<EasyAccessToken>();
		}

	}
}
