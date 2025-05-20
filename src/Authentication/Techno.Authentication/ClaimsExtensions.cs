using System.Security.Claims;

namespace Techno.Authentication
{
    public static class ClaimsExtensions
    {
        public static void Update(this IList<Claim> claims, string type, string value)
        {
            claims.Remove(type);
            claims.Add(new Claim(type, value));
        }

        public static bool Remove(this IList<Claim> claims, string type)
        {
            var claim = claims.FirstOrDefault(c => c.Type == type);
            return claims.Remove(claim!);
        }
    }
}
