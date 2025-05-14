using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace perenne.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CorsEnableAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Garante que os headers de CORS são adicionados em todas as respostas
            var response = context.HttpContext.Response;
            var request = context.HttpContext.Request;
            
            // Remove cabeçalhos existentes para evitar duplicação
            response.Headers.Remove("Access-Control-Allow-Origin");
            response.Headers.Remove("Access-Control-Allow-Methods");
            response.Headers.Remove("Access-Control-Allow-Headers");
            response.Headers.Remove("Access-Control-Allow-Credentials");
            
            // Obtém origem da requisição
            var origin = request.Headers["Origin"].ToString();
            
            // Define a origem permitida, limitar em produção
            response.Headers.Append("Access-Control-Allow-Origin", !string.IsNullOrEmpty(origin) ? origin : "*");
            
            // Permitir credenciais (necessário para autenticação)
            response.Headers.Append("Access-Control-Allow-Credentials", "true");
            
            // Métodos HTTP permitidos
            response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, HEAD");
            
            // Headers permitidos (incluindo aqueles necessários para autenticação)
            response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, Accept, Origin, Pragma, Cache-Control");
            
            // Aumentar o tempo de cache para pre-flight requests (OPTIONS)
            if (request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                response.Headers.Append("Access-Control-Max-Age", "3600");
            }

            base.OnActionExecuting(context);
        }
    }
}
