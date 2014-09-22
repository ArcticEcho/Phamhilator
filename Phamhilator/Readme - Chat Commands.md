<h2>Term Commands:</h2>

 - <code>>>badd term (lq/spam/off/name) (regex term)</code> Adds a new regex term to the specified subfilter of the blacklist (<code>lq</code> = low quality filter, <code>spam</code> = spam filter, <code>off</code> = offensive filter, <code>name</code> = username filter). 

 - <code>>>bremove term (lq/spam/off/name) (regex term)</code> Removes a term from the specified subfilter of the blacklist.
 
 - <code>>>wadd term (lq/spam/off/name) (site, e.g., stackoverflow.com) (regex term)</code> Adds a new term to the specified subfilter of the whitelist.
 
 - <code>>>wremove term (lq/spam/off/name) (site, e.g., stackoverflow.com) (regex term)</code> Removes a term from the specified subfilter of the whitelist.
 
<h2>Tag Commands:</h2>

 - <code>>>add tag (site, e.g., stackoverflow.com) (tag name) (optional: related meta discussion link. E.g., http://meta.stackoverflow.com/questions/0123456/blah-blah-blah)</code> Adds a new tag for the specified site to the Bad Tag Definations.
 
 - <code>>>remove tag (site, e.g., stackoverflow.com) (tag name)</code> Removes a tag from the specified site from the BTD.
 
<h2>FP/TP Commands:</h2>

 - <code>@sam (fp/false/false pos/false positive)</code> (Assuming you're replying to a previous report) registers that the replied-to report was a false positive (which internally alters the score of the terms used to catch said post).
 
 - <code>@sam (tp/true/true pos/true positive)</code> (Assuming you're replying to a previous report) registers that the replied-to report was a true positive.
 
<h2>Misc. Commands:</h2>

 - <code>>>(stats/info)</code> Prints information about the bot.
 
 - <code>>>(help/commands)</code> Prints a link to this page.
 
<br>

<h2>Owner Commands:</h2>

 - <code>>>start</code> Starts (resumes) the bot.
 
 - <code>>>pause</code> Pauses the bot.
 
 - <code>>>add user (user id, e.g., 0123456)</code> Adds a new user to the command access list.

 - <code>>>threshold (percentage, e.g., 12.75)</code> Sets the lower limit of the report accuracy percentage filter (do <i>not</i> include the percentage sign).
 
<br>

 <sup><i>Note: all commands <b>are</b> case sensitive.</i></sup>
