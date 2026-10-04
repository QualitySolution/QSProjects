using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.Cloud.Client;

namespace QS.Cloud.Email.DesktopClient
{
	/// <summary>
	/// Управление токенами бэкенд-клиентов службы емейлов. Используется только из Insider.
	/// </summary>
	public class EmailTokenManagerClient : CloudClientBySession
	{
		public EmailTokenManagerClient(ISessionInfoProvider sessionInfoProvider)
			: base(sessionInfoProvider, DesktopEmailSenderClient.ServiceAddress, DesktopEmailSenderClient.ServicePort)
		{
		}

		private EmailTokenManager.EmailTokenManagerClient Client => new EmailTokenManager.EmailTokenManagerClient(Channel);

		public async Task<IList<BackendToken>> GetTokensAsync(int applicationId)
		{
			var response = await Client.GetTokensAsync(new GetTokensRequest { ApplicationId = applicationId }, headers);
			return response.Tokens;
		}

		public async Task<BackendToken> CreateTokenAsync(int applicationId, string senderName)
		{
			return await Client.CreateTokenAsync(new CreateTokenRequest { ApplicationId = applicationId, SenderName = senderName }, headers);
		}

		public async Task<BackendToken> UpdateTokenAsync(int id, string senderName)
		{
			return await Client.UpdateTokenAsync(new UpdateTokenRequest { Id = id, SenderName = senderName }, headers);
		}

		public async Task DeleteTokenAsync(int id)
		{
			await Client.DeleteTokenAsync(new DeleteTokenRequest { Id = id }, headers);
		}
	}
}
