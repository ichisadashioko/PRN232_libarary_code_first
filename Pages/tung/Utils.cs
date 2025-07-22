namespace tung
{
    public class Utils
    {
        public static string? get_access_token(HttpRequest Request)
        {
            string? existing_token = Request.Headers.Authorization;
            if (string.IsNullOrEmpty(existing_token))
            {
                existing_token = Request.Cookies["token"];
            }

            if (string.IsNullOrEmpty(existing_token))
            {
                return null;
            }

            if (existing_token.StartsWith("Bearer "))
            {
                existing_token = existing_token.Substring("Bearer ".Length);
            }
            return existing_token;
        }

        //public static User? get_user_from_token(string token, HttpClient client, string request_url)
        //{
        //    return null;
        //}
    }
}
