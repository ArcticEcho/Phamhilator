using System.Text;
using System.Linq;



namespace Phamhilator
{
    public static class ReportCleaner
    {
        public static string GetCleanReport(int messageID)
        {
            var report = Stats.PostedReports.First(r => r.Message.ID == messageID);

            var oldTitle = PostFetcher.EscapeString(report.Post.Title, " ");
            var newTitle = CensorString(report.Post.Title);

            var oldName = report.Post.AuthorName;
            var newName = CensorString(report.Post.AuthorName);

            return report.Message.Content.Replace(oldTitle, newTitle).Replace(oldName, newName);
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
