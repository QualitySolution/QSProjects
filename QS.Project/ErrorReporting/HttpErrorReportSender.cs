using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;

namespace QS.ErrorReporting
{
	public class HttpErrorReportSender : IErrorReportSender
	{
		private readonly string serviceAddress;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public HttpErrorReportSender(string serviceAddress)
		{
			this.serviceAddress = serviceAddress;
		}

		public bool SubmitErrorReport(ErrorReport report)
		{
			try
			{
				var reportString = GetPreparedXml(report);

				using (var request = new HttpRequestMessage(HttpMethod.Post, ""))
				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(serviceAddress);
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
					client.DefaultRequestHeaders
						.Add("SOAPAction", "http://tempuri.org/IErrorReportingService/SubmitErrorReport");

					request.Content = new StringContent(reportString, Encoding.UTF8, "text/xml");
					request.Headers.TransferEncodingChunked = null;
					request.Headers.Add("Connection", "Keep-Alive");

					var response = client.PostAsync(request.RequestUri, request.Content).Result;
					return response.IsSuccessStatusCode;
				}
			}
			catch (Exception e)
			{
				logger.Error(e, "Ошибка при выполнении запроса SubmitErrorReport к сервису регистрации ошибок");
				return false;
			}
		}

		private string GetPreparedXml(ErrorReport report)
		{
			string reportString;
			using (var ms = new MemoryStream())
			{
				var formatter = new DataContractSerializer(typeof(ErrorReport));
				formatter.WriteObject(ms, report);
				reportString = Encoding.UTF8.GetString(ms.ToArray());
			}

			reportString = AddPrefixToNodes(reportString);

			reportString =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap"
				+ "/envelope/\"><s:Body><SubmitErrorReport xmlns=\"http://tempuri.org/\">"
				+ reportString
				+ "</SubmitErrorReport></s:Body></s:Envelope>";
			return reportString;
		}

		private string AddPrefixToNodes(string reportString)
		{
			var doc = new XmlDocument();
			var parsedReport = XElement.Parse(reportString);
			var ns = parsedReport.GetDefaultNamespace().ToString();
			var prefix = "d4p1";
			var rootNode = doc.CreateElement(prefix, parsedReport.Name.LocalName, ns);

			foreach (var prefixlessNode in parsedReport.Elements())
			{
				var node = doc.CreateElement(prefix, prefixlessNode.Name.LocalName, ns);
				node.InnerText = prefixlessNode.Value;
				rootNode.AppendChild(node);
			}

			doc.AppendChild(rootNode);
			return doc.OuterXml.Replace($"{prefix}:ErrorReport", "report");
		}
	}
}
