using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class QuoteDetailCommentView
    {
        public int CommentId { get; set; }

        public int QuoteDetailId { get; set; }

        public string Usuario { get; set; }

        public string UserName { get; set; }

        public string Comment { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime DateComment { get; set; }

    }
}
