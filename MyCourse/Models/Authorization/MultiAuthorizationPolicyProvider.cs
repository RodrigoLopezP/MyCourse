using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;


namespace MyCourse.Models.Authorization
{
    /// <summary>Permette chiedere autorizzazione in modalità OR
    ///<example>esempio:
    ///<code>[Authorize(Policy = A, B)]</code>
    ///</example>
    ///</summary>
    public class MultiAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider //eredito il policy provider di default
    {
        private readonly IOptions<AuthorizationOptions> options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MultiAuthorizationPolicyProvider(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> options) : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.options = options;
        }

/*
    dotNet non accetta più policy in modo OR (quindi se la policy A oppure la policy B sono ok, allora autorizzato), solo AND
    questa classe è un POLICY PROVIDER personalizzato, che permette di aggiungere due policy alla nostra action o controller come OR 
*/
        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)//se ottiene la policy con il metodo base, allora ritorna la risposta e fine
            {
                return policy;
            }
            /*es.
            Authorize(Policy = "CourseAuthor , CourseSubscriber")
            se  CourseAuthor oppure CourseSubscriber sono valide, allora ottiene il permesso
            */
            var policyNames = policyName.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim()).ToArray();
            var builder = new AuthorizationPolicyBuilder();
            builder.RequireAssertion(async (context) =>
            {
                var authService = httpContextAccessor.HttpContext.RequestServices.GetService<IAuthorizationService>();
                foreach (var policyName in policyNames)
                {
                    var result = await authService.AuthorizeAsync(context.User, context.Resource, policyName);
                    if (result.Succeeded)
                    {
                        return true;
                    }
                }
                return false;
            });
            return builder.Build();
        }
    }
}