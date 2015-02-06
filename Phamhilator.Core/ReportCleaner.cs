using System.Text;
using System.Linq;



namespace Phamhilator.Core
{
    public static class ReportCleaner
    {
        public static string GetCleanReport(int messageID)
        {
            var report = Stats.PostedReports.First(r => r.Message.ID == messageID);

            var oldTitle = PostFetcher.ChatEscapeString(report.Post.Title, " ");
            var newTitle = CensorString(report.Post.Title);

            var oldName = report.Post.AuthorName;
            var newName = CensorString(report.Post.AuthorName);

            return report.Message.Content.Replace(oldTitle, newTitle).Replace(oldName, newName);
        }



        private static string CensorString(string input)
        {
            var censored = new StringBuilder();
            var nonSpaceI = 0;

            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                {
                    censored.Append(' ');
                }
                else
                {
                    nonSpaceI++;
                    censored.Append(nonSpaceI % 2 == 0 ? '★' : '✩');
                }
            }

            return censored.ToString().Trim();
        }
    }
}
