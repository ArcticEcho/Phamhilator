<h2>Term Commands:</h2>

 - <code>>>badd term (lq/spam/off/name) (regex term here)</code> Adds a new regex term to the specified subfilter of the blacklist (<code>lq</code> = low quality filter, <code>spam</code> = spam filter, <code>off</code> = offensive filter, <code>name</code> = username filter). 

 - <code>>>bremove term (lq/spam/off/name) (term)</code> Removes a term from the subspecified filter of the blacklist.
 
 - <code>>>wadd term (lq/spam/off/name) (site, e.g., stackoverflow.com) (term)</code> Adds a new term to the specified subfilter of the whitelist.
 
 - <code>>>wremove term (lq/spam/off/name) (site, e.g., stackoverflow.com) (term)</code> Removes a term from the specified subfilter of the whitelist.
 
 - <code>>>(+1/uv/upvote) (lq/spam/off/name) (term)</code> Increments the score of the specified trem.
 
 - <code>>>(-1/dv/downvote) (lq/spam/off/name) (term)</code> Decrements the score of the specified trem.
 
<h2>Bot Operation Commands:</h2>

 - <code>>>start</code> Starts (resumes) the bot.
 
 - <code>>>pause</code> Pauses the bot.
 
<h2>Tag Commands:</h2>

 - <code>>>add tag (site, e.g., stackoverflow.com) (tag name)</code> Adds a new tag for the specified site to the Bad Tag Definations.
 
 - <code>>>remove tag (site) (tag name)</code> Removes a tag from the specified site from the BTD.
 
<h2>FP/TP Commands:</h2>

 - <code>@sam (fp/false/false pos/false positive)</code> (Assuming you're replying to a previous report) registers that the replied-to report was a false positive (which internally alters the score of the terms used to catch said post).
 
 - <code>@sam (tp/true/true pos/true positive)</code> (Assuming you're replying to a previous report) registers that the replied-to report was a true positive.
 
<h2>Misc Commands:</h2>

 - <code>>>(stats/info)</code> Prints information about the bot.
 
 - <code>>>(help/commands)</code> Prints a link to this page.
