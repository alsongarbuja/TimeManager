namespace TimeManager.Backend.Utility
{
    public class NavigationHelper
    {
        public static bool IsNavActive(string contextLink, string dataLink)
        {
            string[] contextLinkParts = contextLink.Split("/");
            string[] dataLinkParts = dataLink.Split("?");

            if (contextLinkParts.Length > 2)
            {
                string compareContextLink = RejoinContextLink(contextLinkParts);
                if (dataLinkParts[0].Split("/").Length <= 1)
                {
                    return string.Equals(contextLinkParts[2].ToLower(), dataLinkParts[0], StringComparison.OrdinalIgnoreCase);
                }
                return string.Equals(compareContextLink.Split("?")[0].ToLower(), dataLinkParts[0]+"/".ToLower(), StringComparison.OrdinalIgnoreCase);
            } else
            {
                return string.Equals(contextLinkParts[2].Split("?")[0].ToLower(), dataLinkParts[0].ToLower(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string RejoinContextLink(string[] parts)
        {
            string whole = "";

            for (int i = 2; i < parts.Length; i++)
            {
                whole += parts[i]+"/";
            }

            return whole;
        }
    }
}
