using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AllegiantPDFMergerFinal
{
    class UserMesseging
    {
        private TextBlock messege, tip;

        public string Messege
        {
            set
            {
                if (this.messege != null) this.messege.Text = value;
                Tip = "";
            }
        }

        public string Tip
        {
            set
            {
                if (tip != null) this.tip.Text = value;
            }
        }

        public UserMesseging(TextBlock messege, TextBlock tip)
        {
            this.messege = messege;
            this.tip = tip;
        }
    }
}
