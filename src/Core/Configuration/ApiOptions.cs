namespace Asuka.Configuration
{
    public class ApiOptions
    {
        public string Uri { get; set; }
        public string TagsRoute { get; set; }
        public string ReactionRolesRoute { get; set; }

        public string TagsUri => $"{Uri}{TagsRoute}";
        public string ReactionRolesUri => $"{Uri}{ReactionRolesRoute}";
    }
}
