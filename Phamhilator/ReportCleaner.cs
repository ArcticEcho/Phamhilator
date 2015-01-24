using System.Text;



namespace Phamhilator
{
    public static class ReportCleaner
    {
        public static string GetCleanReport(int messageID)
        {
            var oldTitle = PostFetcher.EscapeString(GlobalInfo.PostedReports[messageID].Post.Title, " ");
            var newTitle = CensorString(GlobalInfo.PostedReports[messageID].Post.Title);

            var oldName = GlobalInfo.PostedReports[messageID].Post.AuthorName;
            var newName = CensorString(GlobalInfo.PostedReports[messageID].Post.AuthorName);

            return GlobalInfo.PostedReports[messageID].Message.Content.Replace(oldTitle, newTitle).Replace(oldName, newName);
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
