using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.ViewModels;
using Org.BouncyCastle.Asn1.Ocsp;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using HttpResponse= PayPalHttp.HttpResponse;//con ImplicitUsings nel csproj, si era creata una ambiguita tra due using, così è stato risolto

namespace MyCourse.Models.Services.Infrastructure
{
     public class PaypalPaymentGateway : IPaymentGateway
     {
          private readonly IOptionsMonitor<PaypalOptions> _options;
          public PaypalPaymentGateway(IOptionsMonitor<PaypalOptions> options)
          {
               _options = options;
          }


          /// <summary>
          ///Tramite il token ottenuto dal GetPaymentUrlAsync
          ///Conferma l'esistenza del pagamento per poi fare la subscribe nel database
          ///</summary>
          public async Task<CourseSubscribeInputModel> CapturePaymentAsync(string token)
          {
               PayPalEnvironment payPalEnvironment = GetPayPalEnvironment(_options.CurrentValue);
               PayPalHttpClient client = new(payPalEnvironment);

               OrdersCaptureRequest request = new(token);
               request.RequestBody(new OrderActionRequest());
               request.Prefer("return=representation");
               try
               {
                    HttpResponse response = await client.Execute(request);

                    Order result = response.Result<Order>();

                    PurchaseUnit purchaseUnit = result.PurchaseUnits.First();
                    Capture capture = purchaseUnit.Payments.Captures.First();

                    //abbiamo creato il customId in questo modo    CustomId=$"{inputModel.CourseId}/{inputModel.UserId}",

                    string[] customIdParts = purchaseUnit.CustomId.Split("/");
                    int courseId = int.Parse(customIdParts[0]);
                    string userId = customIdParts[1];

                    //riottieni il prezzo dal capture
                    var currency = Enum.Parse<Currency>(capture.Amount.CurrencyCode);
                    var amount = decimal.Parse(capture.Amount.Value, CultureInfo.InvariantCulture);

                    //ottieni ora pagamento
                    var paymentDate = DateTime.Parse(capture.CreateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                    return new CourseSubscribeInputModel
                    {
                         CourseId = courseId,
                         UserId = userId,
                         Paid = new(currency, amount),
                         TransactionId = capture.Id,
                         PaymentDate = paymentDate,
                         PaymentType = "Paypal"
                    };
               }
               catch (Exception exc)
               {

                    throw new PaymentGatewayException(exc);
               }

          }

          /// <summary>
          ///Configura tutte le variabilie  dati da mandare a Paypal
          ///Restituisce la URL per fare il pagamento
          ///</summary>
          public async Task<string> GetPaymentUrlAsync(CoursePayInputModel inputModel)
          {
               OrderRequest order = new()         //queste var sono roba del SDK di Paypal che abbiamo installato
               {
                    CheckoutPaymentIntent = "CAPTURE",
                    ApplicationContext = new ApplicationContext()
                    {
                         ReturnUrl = inputModel.ReturnUrl,
                         CancelUrl = inputModel.CancelUrl,
                         BrandName = _options.CurrentValue.BrandName,
                         ShippingPreference = "NO_SHIPPING"
                    },
                    PurchaseUnits = new List<PurchaseUnitRequest>()
                    {
                         new PurchaseUnitRequest(){
                              CustomId=$"{inputModel.CourseId}/{inputModel.UserId}",
                              Description=inputModel.Description,
                              AmountWithBreakdown=new AmountWithBreakdown()
                              {
                                   CurrencyCode=inputModel.Price.Currency.ToString(),
                                   Value=inputModel.Price.Amount.ToString(CultureInfo.InvariantCulture)
                              }
                         }
                    }
               };
               PayPalEnvironment payPalEnvironment = GetPayPalEnvironment(_options.CurrentValue);
               PayPalHttpClient client = new(payPalEnvironment);

               OrdersCreateRequest request = new();
               request.RequestBody(order);
               request.Prefer("return=representation");

               HttpResponse response = await client.Execute(request);

               Order result = response.Result<Order>();
               LinkDescription link = result.Links.Single(link => link.Rel == "approve");
               return link.Href;
          }

          private PayPalEnvironment GetPayPalEnvironment(PaypalOptions options)
          {
               string clientId = options.ClientId;
               string clientSecret = options.ClientSecret;

               return options.Sandbox ? new SandboxEnvironment(clientId, clientSecret) :
                                          new LiveEnvironment(clientId, clientSecret);
          }
     }
}