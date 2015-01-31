using System;



namespace Phamhilator.Core
{
    public class Spammer
    {
        private readonly DateTime creationTime;
        private readonly string site;
        private readonly string name;

        public string Site
        {
            get
            {
                if ((DateTime.UtcNow - creationTime).TotalMinutes > 60)
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
                if ((DateTime.UtcNow - creationTime).TotalMinutes > 60)
                {
                    return "";
                }

                return name;
            }
        }



        public Spammer(string site, string name)
        {
            creationTime = DateTime.UtcNow;

            this.site = site;
            this.name = name;
        }
    }
}
