using System;



namespace Phamhilator.Core
{
    public class ReportedUser
    {
        private readonly DateTime creationTime;
        private readonly string site;
        private readonly string name;

        public string Site
        {
            get
            {
                // Invalidate data after 5 hours.
                if ((DateTime.UtcNow - creationTime).TotalMinutes > 300)
                {
                    return "";
                }

                return site;
            }
        }

        public string Name
        {
            get
            {
                // Invalidate data after 5 hours.
                if ((DateTime.UtcNow - creationTime).TotalMinutes > 300)
                {
                    return "";
                }

                return name;
            }
        }



        public ReportedUser(string site, string name)
        {
            creationTime = DateTime.UtcNow;

            this.site = site;
            this.name = name;
        }
    }
}
