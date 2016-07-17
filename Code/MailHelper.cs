using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IParserPlugin;

namespace Code {

    /// <summary>
    /// Help create email with report
    /// </summary>
    class MailHelper {

        /// <summary>
        /// Create email string from list of units
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public static string ComposeEmailString(List<UnitDetails> units) {
            if (units == null || !units.Any())
                return string.Empty;

            // result string composer
            StringBuilder Result = new StringBuilder();

            for(int i = 0;i<units.Count;i++) {
                // take unit to send
                UnitDetails U = units[i];
                // main block
                Result.Append("<div style=\"border-color:#e5e5e5;border-style:solid;border-width:1px;position:relative;"
                    + "clear:both;margin-bottom:15px;padding:5px;overflow:auto;\">"
                    + "<div style=\"background-color:rgb(224, 255, 255);height:auto;overflow:auto;\">"
                    + "<span style=\"color:gray;float:left;font-size:25px;margin-left:20px;\">"
                    + (i + 1) + "/" + units.Count + "</span>"
                    + "<span style=\"clear:both;font-decoration:none;cursor:pointer;font-size:25px;margin-left:20px;\">&nbsp;&nbsp;<a href=\""
                    + U.Url + "\">"
                    + "<span>" + U.Title + "</span></a>&nbsp;&nbsp;</span>"
                    + "</div>"
                    + "<hr style=\"width:98%;color:gray;\"/><p style=\"clear:both;margin-left:10px;margin-right:15px;\">" 
                    + U.AddText + "</p>"
                    + "<span style=\"font-size:23px;font-weight:bold;float:left;margin-left:15px;\">" 
                    + U.Price + "</span>"
                    + "<span style=\"color:gray;margin-right:100px;font-size:23px;float:right;\">" 
                    + U.TimeAdded.ToLongTimeString() + "</span><hr style=\"width:98%;color:gray;\"/>");

                // adding pictures in block
                foreach (string s in U.Pictures) {
                    Result.Append("<img style=\"max-width:280px;max-height:280px;display:inline-block;margin:10px;float:left;\" src=\"" + s + "\" />");
                }
                // finish block
                Result.Append("<span style=\"clear:both;\">&nbsp;</span>");
                Result.Append("</div>");
            }
            return Result.ToString();
        }
    }
}
