using System.Text;
using System.Windows.Forms;
using Fiddler;

[assembly: RequiredVersion("5.0.0.0")]

namespace CGSSExtension
{
    public class CGSSResponse : Inspector2, IResponseInspector2
    {
        TextBox textbox;
        private byte[] _body;

        public CGSSResponse() { }

        public HTTPResponseHeaders headers { get; set; }

        public byte[] body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;

                if (body != null)
                {
                    string cgssbody = Encoding.UTF8.GetString(body);
                    textbox.Text = CGSSUtil.DecryptBody(cgssbody, CGSSRequest.udid);
                }
            }
        }

        public bool bDirty { get { return false; } }

        public bool bReadOnly { get; set; }

        public void Clear()
        {
            body = null;
            textbox.Text = "";
        }

        public override void AddToTab(TabPage o)
        {
            textbox = new TextBox
            {
                Height = o.Height,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            o.Text = "CGSS Response";
            o.Controls.Add(textbox);
            o.Controls[0].Dock = DockStyle.Fill;
        }

        public override int GetOrder()
        {
            return 150;
        }
    }
}
