using Models.EF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class MomoService
{
	private const string PARTNER_CODE = "MOMO";
	private const string ACCESS_KEY = "F8BBA842ECF85";
	private const string SECRET_KEY = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
	private const string REDIRECT_URL = "http://localhost:58473/momo-return";
	private const string IPN_URL = "http://localhost:58473/momo-ipn";
	private const string REQUEST_TYPE = "payWithMethod";

	private const string ENDPOINT = "https://test-payment.momo.vn/v2/gateway/api/create";

	public async Task<string> CreatePaymentRequest(int amount, String orderID)
	{
		try
		{
			string requestId = PARTNER_CODE + DateTimeOffset.Now.ToUnixTimeMilliseconds();
			string orderId = "MOMO_" + orderID;
			string orderInfo = "Thanh toán MoMo"; // UTF-8
			string extraData = "";


			string rawSignature =
				$"accessKey={ACCESS_KEY}" +
				$"&amount={amount}" +
				$"&extraData={extraData}" +
				$"&ipnUrl={IPN_URL}" +
				$"&orderId={orderId}" +
				$"&orderInfo={orderInfo}" +
				$"&partnerCode={PARTNER_CODE}" +
				$"&redirectUrl={REDIRECT_URL}" +
				$"&requestId={requestId}" +
				$"&requestType={REQUEST_TYPE}";


			// Log raw signature để debug
			Console.WriteLine("RawSignature: " + rawSignature);

			// Tạo HMAC SHA256
			string signature = HmacSHA256(rawSignature, SECRET_KEY);
			Console.WriteLine("Signature: " + signature);

			// Request body JSON
			var requestBody = new
			{
				partnerCode = PARTNER_CODE,
				accessKey = ACCESS_KEY,
				requestId = requestId,
				amount = amount.ToString(), // GIỐNG SPRING BOOT
				orderId = orderId,
				orderInfo = orderInfo,      // KHÔNG ESCAPE
				redirectUrl = REDIRECT_URL,
				ipnUrl = IPN_URL,
				extraData = extraData,
				requestType = REQUEST_TYPE,
				signature = signature
			};


			var jsonBody = JsonConvert.SerializeObject(requestBody);
			Console.WriteLine("RequestBody: " + jsonBody);

			using (var client = new HttpClient())
			{
				// Đảm bảo TLS1.2
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

				var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(ENDPOINT, content);

				string responseText = await response.Content.ReadAsStringAsync();
				Console.WriteLine("MoMo Response: " + responseText);

				JObject json = JObject.Parse(responseText);

				if ((int)json["resultCode"] != 0)
				{
					Console.WriteLine("MoMo create error: " + json["message"]);
					return null;
				}

				return json["payUrl"]?.ToString();

			}
		}
		catch (Exception ex)
		{
			// Log đầy đủ stack trace để debug
			Console.WriteLine("Momo error: " + ex.ToString());
			return null;
		}
	}

	private string HmacSHA256(string data, string key)
	{
		if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(key))
			throw new ArgumentException("Data or key is null or empty!");

		using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
		{
			byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
			return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
		}
	}


}
