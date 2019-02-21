using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralClustering
{
    public class telegramcrw_message
    {
        public string hashRes { get; set; }
        public string fK_FromProfileId { get; set; }
        public string language { get; set; }
        public string id { get; set; }
        //Needed for Trend Detector
        public string messageDate { get; set; }
        public string messageTextCleaned { get; set; }
        public string createDate { get; set; }

        public List<string> concept { get; set; }

        public telegramcrw_message()
        {
            concept = new List<string>();
            conceptsIDs = new List<int>();
            scence = new List<string>();
        }

        //Added In this Program
        public List<int> conceptsIDs { get; set; }
        public List<string> scence { get; set; }
        public string public_scence { get; set; }

        ////Not Needed For Now
        //public string messageText { get; set; }
        //public string cleanText { get; set; }
        //public long crawlerFromUserID { get; set; }
        //public string createdAt { get; set; }
        //public string crwDate { get; set; }
        //public string deletedAt { get; set; }
        //public long fK_InReplyToStatusID { get; set; }
        //public long fK_InReplyToUserID { get; set; }
        //public int fK_LanguageId { get; set; }
        //public long fK_QuotedStatusID { get; set; }
        //public long fK_RetweetStatusID { get; set; }
        //public long fK_UserID { get; set; }
        //public int favoriteCount { get; set; }
        //public string hashCText { get; set; }
        //public string hashText { get; set; }
        //public long identityNo { get; set; }
        //public string insertAt { get; set; }

        //public bool isDeleted { get; set; }
        //public bool isFavorited { get; set; }
        //public string lang { get; set; }
        //public long pK_StatusId { get; set; }
        //public int retweetCount { get; set; }
        //public string source { get; set; }
        //public string text { get; set; }
        //public int type { get; set; }

    }
}
