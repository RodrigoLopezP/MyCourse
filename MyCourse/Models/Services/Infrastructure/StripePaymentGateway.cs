using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using Stripe;
using Stripe.Checkout;

namespace MyCourse.Models.Services.Infrastructure
{
     public class StripePaymentGateway : IPaymentGateway
     {
          public IOptionsMonitor<StripeOptions> _stripeOptions { get; }

          public StripePaymentGateway(IOptionsMonitor<StripeOptions> stripeOptions)
          {
               _stripeOptions = stripeOptions;

          }
          public async Task<string> GetPaymentUrlAsync(CoursePayInputModel inputModel)
          {
               SessionCreateOptions sessionCreateOptions = new()
               {
                    ClientReferenceId = $"{inputModel.CourseId}/{inputModel.UserId}",
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions()
                        {
                            Name = inputModel.Description,
                            Amount = Convert.ToInt64(inputModel.Price.Amount * 100),
                            Currency = inputModel.Price.Currency.ToString(),
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    PaymentIntentData = new SessionPaymentIntentDataOptions
                    {
                         CaptureMethod = "manual"//indico che voglio controllare manualmente che il pagamento è stato fatto con successo(con il metodo capture)
                    },
                    PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                    SuccessUrl = inputModel.ReturnUrl + "?token={CHECKOUT_SESSION_ID}",
                    CancelUrl = inputModel.CancelUrl
               };

               RequestOptions requestOptions = new()
               {
                    ApiKey = _stripeOptions.CurrentValue.PrivateKey
               };

               SessionService sessionService = new();
               Session session = await sessionService.CreateAsync(sessionCreateOptions, requestOptions);
               return session.Url;
          }

          public async Task<CourseSubscribeInputModel> CapturePaymentAsync(string token)
          {
               try
               {
                    RequestOptions requestOptions = new()
                    {
                         ApiKey = _stripeOptions.CurrentValue.PrivateKey
                    };

                    SessionService sessionService = new();
                    Session session = await sessionService.GetAsync(token, requestOptions: requestOptions);//è per indicare che il secondo param si riferisce al param REQUESTOPTIONS

                    PaymentIntentService paymentIntentService = new();
                    PaymentIntent paymentIntent = await paymentIntentService.CaptureAsync(session.PaymentIntentId, requestOptions: requestOptions);

                    string[] customIdParts = session.ClientReferenceId.Split('/');
                    int courseId = int.Parse(customIdParts[0]);
                    string userId = customIdParts[1];

                    //dati di Money, assegni separamente solo perché tutto si veda più ordinato
                    var currency = Enum.Parse<Currency>(paymentIntent.Currency, ignoreCase: true);
                    var amount = paymentIntent.Amount / 100m;// paymentIntent.Amount è in centesimi, quindi fdobbiamo convertirlo in euro divendo per 100
                    return new CourseSubscribeInputModel
                    {
                         CourseId = courseId,
                         UserId = userId,
                         Paid = new(currency, amount),
                         TransactionId = paymentIntent.Id,
                         PaymentDate = paymentIntent.Created,
                         PaymentType = "Stripe"
                    };
               }
               catch (Exception exc)
               {
                    throw new PaymentGatewayException(exc);
               }
          }
     }
}