using System.Text;



namespace Phamhilator
{
    public static class ReportCleaner
    {
        public static string GetCleanReport(int messageID)
        {
            var oldTitle = PostFetcher.EscapeString(Stats.PostedReports[messageID].Post.Title, " ");
            var newTitle = CensorString(Stats.PostedReports[messageID].Post.Title);

            var oldName = Stats.PostedReports[messageID].Post.AuthorName;
            var newName = CensorString(Stats.PostedReports[messageID].Post.AuthorName);

            return Stats.PostedReports[messageID].Message.Content.Replace(oldTitle, newTitle).Replace(oldName, newName);
        }



        private static string CensorString(string input)
        {
            var censored = new StringBuilder();

            foreach (var c in input)
            {
                censored.Append(c == ' ' ? ' ' : '★');
            }

            return censored.ToString();
        }
    }
}
